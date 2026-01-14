namespace asp_net_core_no_template.Services
{
    public class RequestCounter
    {
        private int _count = 0;

        public int Increment()
        {
            _count++;
            return _count;
        }
    }
}
