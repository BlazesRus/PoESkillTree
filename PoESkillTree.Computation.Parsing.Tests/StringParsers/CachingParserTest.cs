﻿using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing.StringParsers;

namespace PoESkillTree.Computation.Parsing.Tests.StringParsers
{
    [TestFixture]
    public class CachingParserTest
    {
        private const string TrueStat = "true";
        private const string TrueRemaining = "trueRemaining";
        private const string TrueParsed = "trueParsed";
        private const string FalseStat = "false";
        private const string FalseRemaining = "falseRemaining";
        private const string FalseParsed = "falseParsed";

        private Mock<IStringParser<string>> _innerMock;
        private IStringParser<string> _inner;

        [SetUp]
        public void SetUp()
        {
            _innerMock = new Mock<IStringParser<string>>();
            _innerMock.Setup(p => p.Parse(TrueStat))
                .Returns(new StringParseResult<string>(true, TrueRemaining, TrueParsed));
            _innerMock.Setup(p => p.Parse(FalseStat))
                .Returns(new StringParseResult<string>(false, FalseRemaining, FalseParsed));
            _inner = _innerMock.Object;
        }

        [Test]
        public void IsIParserString()
        {
            var sut = new CachingParser<string>(_inner);

            Assert.IsInstanceOf<IStringParser<string>>(sut);
        }

        [Test]
        public void IsIParserInt()
        {
            var sut = new CachingParser<int>(Mock.Of<IStringParser<int>>());

            Assert.IsInstanceOf<IStringParser<int>>(sut);
        }

        [TestCase(TrueStat, ExpectedResult = true)]
        [TestCase(FalseStat, ExpectedResult = false)]
        public bool TryParsePassesSuccessfullyParsed(string stat)
        {
            var sut = new CachingParser<string>(_inner);

            var (actual, _, _) = sut.Parse(stat);

            return actual;
        }

        [TestCase(TrueStat, ExpectedResult = TrueRemaining)]
        public string TryParsePassesRemaining(string stat)
        {
            var sut = new CachingParser<string>(_inner);

            var (_, actual, _) = sut.Parse(stat);

            return actual;
        }

        [TestCase(TrueStat, ExpectedResult = TrueParsed)]
        public string TryParsePassesResult(string stat)
        {
            var sut = new CachingParser<string>(_inner);

            var (_, _, actual) = sut.Parse(stat);

            return actual;
        }

        [Test]
        public void TryParseCachesSingleStat()
        {
            var sut = new CachingParser<string>(_inner);

            sut.Parse(TrueStat);
            sut.Parse(TrueStat);

            _innerMock.Verify(p => p.Parse(TrueStat), Times.Once);
        }

        [Test]
        public void TryParsesCachesMultipleStats()
        {
            var sut = new CachingParser<string>(_inner);

            sut.Parse(TrueStat);
            sut.Parse(FalseStat);
            sut.Parse(FalseStat);
            sut.Parse("whatever");
            sut.Parse(TrueStat);
            sut.Parse(TrueStat);
            sut.Parse("whatever");

            _innerMock.Verify(p => p.Parse(TrueStat), Times.Once);
            _innerMock.Verify(p => p.Parse(FalseStat), Times.Once);
            _innerMock.Verify(p => p.Parse("whatever"), Times.Once);
        }
    }
}