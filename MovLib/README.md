# CoreLib

## Performance Side Note

Programs aren't going to be very large (at most a couple thousand instructions, with some "end game" programs maybe consisting of 10s of thousands if fully expanded) so we don't really have to worry about fast parsing but there are a few things we do.

- Caching, programs are cached based on their MD5 hash this hash includes the current language version (which should be incremented if language changes are incompatible with currently built programs) this will help since most programs are built ontop of previously built programs (so the style is very incremental).
- One-pass, since the language is so trivial we do everything in a "single" pass (we skip AST/other IRs and output direct bytecode), we could technically execute directly the program but the main benefit of the bytecode is performance (since the code is very jump heavy storing byte instructions in a list makes jumps performant).

In terms of execution the core loop being so low level does hurt execution performance, for example a simple add instruction (not even handling negatives) would be as follows:

```
# presuming r1 = r1 + r2
# (which isn't not even that realistic since
   typically it uses an args array but oh well)
add:
    # while r2 > 0
    # neg = !r2
    PC = INV_BOOL[r2] ? END
    # inc = r1 + 1
    r1 = INC[r1]
    # dec = r2 + 1
    r2 = DEC[r2]
    PC = add
END:
    # return back using RET register
    PC = RET
```

This costs a minimum of 5 instructions (well 2 if it does no work) not including the jump into this function / setup of arguments (also this elides argument handling).

> You could write this quite a bit more performant by using multiple INC/DEC tables that use higher numbers (i.e. if r2 > 100 subtract 100 first rather than always 1).

Regardless, even with a base10 series of tables (and the maximum value of 9,999) it will take $9*4 = 36$ loops, of course this is a significant improvement over 9,999 as it currently stands.

> Another note is that keep in mind it's very hard to "compare" two values without subtracting them (as expensive as addition) so we can't do tricks like if first number is < second number swap addition order.  The most that is typically done is by checking if the first number > 5,000 then it swaps (or just by being very careful with order of arguments).  Also keep in mind the more checks you do the more expensive you make simple addition.

Overall, this means that Mov programs have a different performance profile than most languages:
- Much more heavy interpreter loops (each instruction does significantly less work)
- Most memory accesses are read heavy (and write lighter) outside of constant writing to registers (`r1 = 2`) any instruction will involve at-least 1 read (if not more) and at-most 1 write.
- Code is "branchless" with the exception of conditional moves.

> Out of interest you can implement "branchless" moves by doing this (presuming IF is a 2 byte array).  I heavily emphasise it as "branchless" (with intended quotes) because there are of course 2 branches here and given the jump isn't known it isn't branchless.  But this isn't super realistic as I've said below for 99% of conditional moves.

```
# given <TARGET> = <COND> ? <VAL>
IF[1] = SET
# If TARGET is PC 
IF[0] = ELSE
# Bool maps [0] = 0, and everything else to 1
PC = IF[BOOL[<COND>]]
SET:
<TARGET> = VAL
ELSE:
```

> This is technically not the most optimal, since in some cases you can utilize the properties of the values, but it has to be written this way to handle cases like `OUT = r1 ? 1` where `OUT` is a write-only pin (so we have to ensure we don't write to it).  So given you can do safe reads (with no side-effects since there are move triggered instructions) you can do truly branchless jumps.

```
# given <TARGET> = <COND> ? <VAL>
# where <TARGET> is not PC & not a write-only PIN or a read PIN with side-effects
IF[0] = <TARGET>
IF[1] = <VAL>
<TARGET> = IF[BOOL[<COND>]]

# given PC = <COND> ? <VAL>
# i.e. where <TARGET> is PC
# this is not branchless (same issues as above) but saves an extra jump by inlining the SET/ELSE cases.
IF[1] = <VAL>
# also could be INC_2[PC]
IF[0] = ELSE
PC = IF[BOOL[<COND>]]
ELSE:
```

### Performance Optimizations

#### Function Caching

Common functions like ADD/SUB/... are heavily cached behind the scenes.

This is done transparently and only when debugging isn't enabled.  The way we do this is by tracking how many instructions your program executes for every integer combination (which is only 400 million... ish values for add/sub/...) while also caching the value, this lookup is then used when fetching the value in the future (incrementing instruction counters and other stats too).
> Technically, this cache is incremental and isn't built fully at the beginning of the program.

The size of this cache isn't that big;
- Result which typically is the arguments/other state
- We need 2 16 bit ints to store -9,999 to 9,999 for both args
- We need to also store instruction count & other costs (memory read/written)

All together it could easily be upwards of 16-24 bytes... this would be a 9MB+ table *per function* at most.  Even incrementally cached that's massive!  But we don't need to keep the entire table, each table typically caches the last state and we store up to 1 MB per function.  The lookups are done by a hashtable and we don't actually have an eviction policy we just *overwrite* any collision, this functionally acts as a reasonable enough eviction policy.

> We could just "compress" your add/sub/... programs into `+`/`-`/... but we would lose the other state you modify and the instruction count (/ other counts). We could try to build a function to model the values (but it would likely be stepwise and quite complicated).  Ideally, you could use a binary tree and calculate the IC/others based on a linear set of ranges (but honestly why).

All together, I limit the cache *very* heavily because an over-run cache isn't fun but this is configurable in settings (and we choose a reasonable default based on RAM available).  While we *could* cache all functions, we are also pretty limited in what we will do (just because most functions aren't likely to be heavily called and our caching support).

> Note: any "impure" function (i.e. external state) isn't allowed and functions aren't allowed to "read" memory they've not written (with the exception of arguments).

We actually do support impure functions (since we want to support optimal versions of `add` that avoid the couple instruction overhead of using `args`) we just "promote" them to being pure by having all their "read" memory cells be encoded as part of arguments.  This technically even works for arbitrary memory reads.

> Functions do track a hit/miss rate, and if the rate goes < 10% then the function typically will have it's cached dropped.  Caches get created for the hottest functions (in order of most "hot") skipping those with bad cache rates (so once one gets dropped typically the next function gets picked) though this doesn't include very complex functions (since there is a memory limit on how large each "state" can be) and functions that are called < 10 times.

This caching funnily enough leads to us having pretty "fast" naive fibonnacci functions from a time execution standpoint despite the code implementation maybe not having actual caching.  For example, below is faster (in terms of time to run) then if it actually had caching (due to caching requiring extra logic to maintain a cache ontop of our cache).

```
# FIB(r1)
FIB:
    # table implementing <= 1
    # (typical for most solutions to have a couple tables)
    # in this case you could implement entire fib with a table :)
    # (and ideally would handle at-least the first couple dozen).
    PC = LE_1[r1] ? IF_SIMPLE

    # else rout = FIB(r1 - 1) + FIB(r1 - 2)
    r1 = DEC[r1]
    # save arg
    SP = INC[SP]
    SP[0] = r1
    PC = FIB
    
    # save rout
    SP = INC[SP]
    SP[0] = rout

    # -2 (already -1'd)
    r1 = DEC[SP[-1]]
    PC = FIB

    # rout = ^^ + ^^
    args[0] = rout
    args[1] = SP[0]
    PC = ADD

    # restore SP & r1
    SP = DEC[SP]
    r1 = INC[SP[0]]
    SP = DEC[SP]

    # rout is already set
    PC = RET

IF_SIMPLE:
    # 0 = 0, 1 = 1
    rout = r1
    PC = RET
```

This is a weird sort of "side-effect" and typically we try to avoid problems that have a "faster" solution which is just caching.

> Note: avoiding caching as a fast solution to the problem is also just better puzzle design in most cases this is because;
> 
> Caching is "boring" and typically the entire problem in those cases can just be stuffed into a big table (especially for a single input argument like factorial/fibonacci).  Choosing clever tables for cases like LE_1 or handling first N values (since they are more common) is interesting but trivialising an entire problem with a table is boring.