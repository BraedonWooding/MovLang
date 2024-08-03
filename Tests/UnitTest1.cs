namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        const string SIMPLE_MOV = @"
            r1 = 2
            r2 = 10
            r1 = r2
        ";

        [TestMethod]
        public void TestStandardSetup(string command)
        {

        }
    }
}