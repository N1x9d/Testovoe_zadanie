using NetMQ;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ClientTamplate
    {
        private string _text;
        private AsynchronousClient _client;
        private string _path;
        private string responce = string.Empty;

        public string Responce { get => responce; private set => responce = value; }
        public string Text { get => _text; private set => _text = value; }
        public string Path { get => _path; private set => _path = value; }

        public ClientTamplate(string Text, string Path = "")
        {
            this.Text = Text;
            this.Path = Path;
            _client = new AsynchronousClient();


            
        }

        public async void WaitResalt()
        {
            
              var a = _client.StartClient(Text);
            //Thread.Sleep(1000);
            while (a == string.Empty || _client.ServerOverload)
            {
                if (_client.ServerOverload)
                {

                     a=_client.StartClient(Text);
                   
                }
               // Thread.Sleep(1000);
            }
            Responce = a;
        }
    }
}
