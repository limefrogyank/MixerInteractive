using System;
using System.Collections.Generic;
using System.Text;

namespace MixerInteractive.State
{
    public class StateFactory
    {
        private Client _client;

        public Scene CreateScene(SceneData sceneData)
        {
            var scene = new Scene(sceneData);
            scene.SetClient(_client);
            return scene;
        }

        public void SetClient(Client client)
        {
            _client = client;
        }
    }
}
