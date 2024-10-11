using ScorePercentage.Patches;
using System;
using Zenject;

namespace ScorePercentage.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ResultsViewData>().AsSingle();
        }
    }
}
