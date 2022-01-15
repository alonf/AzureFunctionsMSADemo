namespace UI
{
    class ApiAccessor
    {
        private readonly APIDemo.Client _client = new APIDemo.Client(new HttpClient());
        public async Task<bool> SendAsync(string name, int value)
        {
            try
            {
                await _client.SendAsync(new APIDemo.Data { Name = name, Value = value });
                return true;
            }
            catch (Exception)
            {
                //todo: log
                return false;
            }
        }
    }
}