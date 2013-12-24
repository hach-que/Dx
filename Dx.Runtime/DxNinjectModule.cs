using Ninject.Modules;

namespace Dx.Runtime
{
    public class DxNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IObjectWithTypeSerializer>().To<DefaultObjectWithTypeSerializer>();

            this.Bind<IMessageIO>().To<DefaultMessageIO>();
            this.Bind<IMessageConstructor>().To<DefaultMessageConstructor>();


        }
    }
}
