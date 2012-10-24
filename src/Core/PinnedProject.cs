namespace XRepo.Core
{
    public class PinnedProject
    {
        public IPin Pin { get; set; }
        public RegisteredProject Project { get; set; }
    }
}