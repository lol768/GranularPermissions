using System;
using System.Text;
using GranularPermissions.Conditions;
using GranularPermissions.Tests.Stubs;
using Loyc.Syntax;
using Loyc.Syntax.Les;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Shouldly;

namespace GranularPermissions.Tests
{
    [TestFixture]
    public class ConditionParserTests
    {
        [Test]
        public void TestSimple()
        {
            var sut = new ConditionEvaluator();
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("0 < 1")).ShouldBe(true);
        }
        
        [Test]
        public void TestLogic()
        {
            var sut = new ConditionEvaluator();
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("false && true")).ShouldBe(false);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("true && true")).ShouldBe(true);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("true && false")).ShouldBe(false);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("false && false")).ShouldBe(false);
            
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("false || true")).ShouldBe(true);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("true || true")).ShouldBe(true);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("true || false")).ShouldBe(true);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("false || false")).ShouldBe(false);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("!false")).ShouldBe(true);
            sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("!true")).ShouldBe(false);
        }

        [Test]
        public void TestJavaScriptStyleInsanity()
        {
            // arrange
            var sut = new ConditionEvaluator();
            
            // act (ish)
            ActualValueDelegate<bool> del = () => sut.Evaluate(null, Les2LanguageService.Value.ParseSingle("!5"));
            
            // assert
            Assert.That(del, Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void TestPropertyReference()
        {
            var sut = new ConditionEvaluator();
            var product = new Product
            {
                Name = "Huel",
                Category = new Category
                {
                    CategoryId = 5
                }
            };
            
            sut.Evaluate(product, Les2LanguageService.Value.ParseSingle("resource.Category.CategoryId == 5")).ShouldBe(true);
        }

        [Test]
        public void TestStackOverflow()
        {
            var sut = new ConditionEvaluator();
            var product = new Product
            {
                Name = "Huel",
                Category = new Category
                {
                    CategoryId = 5
                }
            };

            product.Category.ProductReference = product;

            var code = new StringBuilder("resource");
            for (var i = 0; i < 100; i++)
            {
                code.Append(".Category.ProductReference");
            }

            code.Append(@".Name == ""Huel""");
            ActualValueDelegate<bool> del = () => sut.Evaluate(product, Les2LanguageService.Value.ParseSingle(code.ToString()));
            Assert.That(del, Throws.TypeOf<StackOverflowException>());
        }

    }

}