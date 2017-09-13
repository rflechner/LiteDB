using LiteDB.FSharp.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiteDB.Tests.Mapper
{
    [TestClass]
    public class Basic_Fsharp_Mapper_Tests
    {
        [TestMethod, TestCategory("Mapper")]
        public void Basic_Mapper()
        {
            var mapper = BsonMapper.Global;

            var customer = ClientCustomer.Build(1, "toto", new []{"+336123456789", "+3313456789"});

            var doc = mapper.ToDocument(customer);
            //var clientCustomer = mapper.ToObject<ClientCustomer>(doc);
        }
    }
}
