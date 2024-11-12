using Microsoft.Playwright;

namespace E2ETests
{
    abstract public class BenchmarkLabBaseTest: PageTest
    {
        public override BrowserNewContextOptions ContextOptions()
        {
            return TestConfig.ContextOptions();
        }
    }
}
