using Ninject.Modules;

namespace cslib
{
    public class CSharpLibraryNinjectModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IInstrumenter>().To<DefaultInstrumenter>().InSingletonScope();
            
            this.Bind<IProjectDiscovery>().To<DefaultProjectDiscovery>();
            this.Bind<ILinter>().To<BuiltinLinter>();
            this.Bind<ILinter>().To<StyleCopLinter>();
        }
    }
}

