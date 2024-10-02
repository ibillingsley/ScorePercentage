using ScorePercentage.Patches;
using System;
using Zenject;

namespace ScorePercentage.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<LevelStatsViewPatches>().AsSingle();
            Container.BindInterfacesTo<ResultsViewControllerPatches>().AsSingle();
        }
    }
}
