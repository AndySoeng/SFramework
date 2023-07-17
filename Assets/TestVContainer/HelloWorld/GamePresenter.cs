using VContainer;
using VContainer.Unity;

namespace MyGame
{
    public class GamePresenter : IStartable
    {
        readonly HelloWorldService helloWorldService;
        readonly HelloScreen helloScreen;


        public GamePresenter(
            HelloWorldService helloWorldService,
            HelloScreen helloScreen)
        {
            this.helloWorldService = helloWorldService;
            this.helloScreen = helloScreen;
        }

        public void Start()
        {
            helloScreen.HelloButton.onClick.AddListener(() => helloWorldService.Hello());
        }

        void Tick()
        {
            helloWorldService.Hello();
        }
    }
}