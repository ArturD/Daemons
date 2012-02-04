using Daemons.ServiceLocators;
using NUnit.Framework;

namespace Daemons.Tests.ServiceLocators
{
    [TestFixture]
    public class ServiceLocatorTests
    {
        [Test]
        public void RegisterAndResolveInstanceByInterface()
        {
            var serviceLocator = new ServiceLocator();
            var a = new A();

            serviceLocator.RegisterInstance(typeof(IA), a);
            var result = serviceLocator.GetService(typeof (IA));

            Assert.AreEqual(a, result);
        }

        [Test]
        public void RegisterAndResolveInstanceByClass()
        {
            var serviceLocator = new ServiceLocator();
            var a = new A();

            serviceLocator.RegisterInstance(typeof(A), a);
            var result = serviceLocator.GetService(typeof(A));

            Assert.AreEqual(a, result);
        }


        [Test]
        public void RegisterAndResolveTypeByInterface()
        {
            var serviceLocator = new ServiceLocator();
            
            serviceLocator.RegisterSingleton(typeof(IA), typeof(A));
            var result = serviceLocator.GetService(typeof(IA));

            Assert.NotNull(result);
            Assert.IsInstanceOf<A>(result);
        }

        [Test]
        public void RegisterAndResolveTypeByClass()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterSingleton(typeof(A), typeof(A));
            var result = serviceLocator.GetService(typeof(A));

            Assert.NotNull(result);
            Assert.IsInstanceOf<A>(result);
        }

        [Test]
        public void AvoidCircularReferences()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterSingleton(typeof(IA), typeof(A));
            serviceLocator.RegisterSingleton(typeof(IB), typeof(B));
            serviceLocator.RegisterSingleton(typeof(IC), typeof(C));
            var result = (IC) serviceLocator.GetService(typeof(IC));

            Assert.NotNull(result);
            Assert.IsInstanceOf<C>(result);
            Assert.AreEqual(3, result.UsedConstructor);
        }

        [Test]
        public void UseProperConstructorWithOneParameter()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterSingleton(typeof(IA), typeof(A));
            serviceLocator.RegisterSingleton(typeof(IC), typeof(C));
            var result = (IC)serviceLocator.GetService(typeof(IC));

            Assert.NotNull(result);
            Assert.IsInstanceOf<C>(result);
            Assert.AreEqual(2, result.UsedConstructor);
        }

        [Test]
        public void UseProperConstructorWithNoParameters()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterSingleton(typeof(IC), typeof(C));
            var result = (IC)serviceLocator.GetService(typeof(IC));

            Assert.NotNull(result);
            Assert.IsInstanceOf<C>(result);
            Assert.AreEqual(1, result.UsedConstructor);
        }

        [Test]
        public void SingletonInstance()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterSingleton(typeof(IA), typeof(A));
            var result = serviceLocator.GetService(typeof(IA));
            var result2 = serviceLocator.GetService(typeof(IA));

            Assert.NotNull(result);
            Assert.IsInstanceOf<A>(result);
            Assert.AreEqual(result, result2);
        }


        [Test]
        public void Transient()
        {
            var serviceLocator = new ServiceLocator();

            serviceLocator.RegisterTransient(typeof(IA), typeof(A));
            var result = serviceLocator.GetService(typeof(IA));
            var result2 = serviceLocator.GetService(typeof(IA));

            Assert.NotNull(result);
            Assert.IsInstanceOf<A>(result);
            Assert.NotNull(result2);
            Assert.IsInstanceOf<A>(result2);
            Assert.AreNotEqual(result, result2);
        }

        [Test]
        public void ResolveAdHocTransientInstantIfNotRegistered()
        {
            var serviceLocator = new ServiceLocator();

            var result = serviceLocator.GetService(typeof(A));
            var result2 = serviceLocator.GetService(typeof(A));

            Assert.NotNull(result);
            Assert.IsInstanceOf<A>(result);
            Assert.NotNull(result2);
            Assert.IsInstanceOf<A>(result2);
            Assert.AreNotEqual(result, result2);
        }

        public interface IA
        { 
        }

        public interface IB
        {
        }

        public class A : IA
        {   
        }

        public class B : IB
        {
            public B(IA a)
            {
            }
        }

        public interface IC
        {
            int UsedConstructor { get; set; }
        }

        public class C : IC
        {
            public int UsedConstructor { get; set; }
            public C()
            {
                UsedConstructor = 1;
            }
            public C(IA a)
            {
                UsedConstructor = 2;
            }

            public C(IA a, IB b)
            {
                UsedConstructor = 3;
            }
            public C(IA a, IB b, IC c)
            {
                UsedConstructor = 4;
            }
        }
    }
}
