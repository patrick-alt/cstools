using Ninject.Modules;

namespace cslib
{
    public class CSharpLibraryNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IInstrumenter>().To<DefaultInstrumenter>().InSingletonScope();
        }
    }
}

