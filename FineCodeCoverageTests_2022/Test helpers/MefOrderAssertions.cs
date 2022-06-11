namespace FineCodeCoverageTests
{
    using System;
    using System.Linq;
    using FineCodeCoverage.Core.Utilities;
    using NUnit.Framework;

    public static class MefOrderAssertions
    {
        private static FineCodeCoverage.Core.Utilities.OrderAttribute GetOrderAtrribute(Type classType) =>
            classType.GetTypedCustomAttributes<FineCodeCoverage.Core.Utilities.OrderAttribute>(false)[0];
        public static void TypeHasExpectedOrder(Type classType, int expectedOrder) =>
            Assert.That(expectedOrder, Is.EqualTo(GetOrderAtrribute(classType).Order));

        public static void InterfaceExportsHaveConsistentOrder(Type interfaceType)
        {
            var types = interfaceType.Assembly.GetTypes();
            var derivations = types.Where(t => t != interfaceType && interfaceType.IsAssignableFrom(t));
            var orders = derivations.Select(d =>
            {
                var orderAttribute = GetOrderAtrribute(d);
                if (orderAttribute == null)
                {
                    throw new Exception("Missing mef attribute");
                }
                if (orderAttribute.ContractType != interfaceType)
                {
                    throw new Exception("Incorrect contract type");
                }
                return orderAttribute.Order;
            }).OrderBy(i => i).ToList();

            Assert.That(orders, Is.Not.Empty);
            var count = 1;
            foreach (var order in orders)
            {
                Assert.That(count, Is.EqualTo(order));
                count++;
            }
        }
    }
}
