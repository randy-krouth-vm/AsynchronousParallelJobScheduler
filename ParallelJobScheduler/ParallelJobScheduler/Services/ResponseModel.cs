namespace ParallelJobScheduler
{
    internal class ResponseModel
    {
        internal string response { get; set; } = "";

        internal ResponseModel() {  }

        internal ResponseModel(string response)
        {
            this.response = response;
        }
    }
}
