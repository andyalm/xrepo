namespace XRepo.Installer
{
    interface IInstallable
    {
        void Install(string buildTargetsDirectory);
        void Uninstall();
    }
}