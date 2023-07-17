using MyGame;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] HelloScreen helloScreen;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<HelloWorldService>(Lifetime.Singleton);

        RegisterEntryPoint(builder);
        builder.RegisterComponent(helloScreen);
    }

    private void RegisterEntryPoint(IContainerBuilder builder)
    {
        //1. RegisterEntryPoint
        //相当于注册了所有的类Mono事件
        builder.RegisterEntryPoint<GamePresenter>();

        //2.Register with Lifetime
        //As哪个接口，注册哪个事件
        //builder.Register<GamePresenter>(Lifetime.Singleton).As<ITickable>().As<IStartable>();

        //3. UseEntryPoints
        //相当于注册了所有的类Mono事件
        /*
        builder.UseEntryPoints(Lifetime.Singleton, entryPoints =>
        {
            entryPoints.Add<GamePresenter>();
            // entryPoints.Add<OtherSingletonEntryPointA>();
            // entryPoints.Add<OtherSingletonEntryPointB>();
            // entryPoints.Add<OtherSingletonEntryPointC>();
        });
        */
    }
}