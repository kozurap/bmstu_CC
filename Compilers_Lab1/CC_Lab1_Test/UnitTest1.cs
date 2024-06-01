using Compilers_Lab1;
using NUnit.Framework;

namespace CC_Lab1_Test
{
    public class Tests
    {
        List<DeterministicFiniteAutomaton> automatons = new List<DeterministicFiniteAutomaton>();
        string[] strings1True = new[] { "zr", "ababbaazr", "aaaaaaaazr" };
        string[] strings1False = new[] { "zrazr", "t" };
        string[] strings2True = new[] { "ababbaabbbabbbaaababbb", "ababbaabbbabbbaaababb", "ababbaabbbabbbaaababbbb" };
        string[] strings2False = new[] { "ababbaabbbabbbaaabab", "x" };
        string[] strings3True = new[] { "ab", "abzabzab", "abzab" };
        string[] strings3False = new[] { "ababz", "r" };
        string[] strings4True = new[] { "aa", "aabba", "bb" };
        string[] strings4False = new[] { "aabbab", "aavvvv" };

        [SetUp]
        public void Setup()
        {

        }
        
        public void TestRegex1()
        {
            var engine = new RegexEngineService("(a|b)*zr");
            foreach (var str in strings1True)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(true));
            }
            foreach (var str in strings1False)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRegex2()
        {
            var engine = new RegexEngineService("(a|b)*a?b*(a|b)?b*(aa)(aba)*b?bbb?");
            foreach (var str in strings2True)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(true));
            }
            foreach (var str in strings2False)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRegex3()
        {
            var engine = new RegexEngineService("(abz)*(abz)*ab");
            foreach (var str in strings3True)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(true));
            }
            foreach (var str in strings3False)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRegex4()
        {
            var engine = new RegexEngineService("((a*bb(a|b)?)|a*)");
            foreach (var str in strings4True)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(true));
            }
            foreach (var str in strings4False)
            {
                var answer = engine.Simulate(str);
                Assert.That(answer, Is.EqualTo(false));
            }
        }
    }
}