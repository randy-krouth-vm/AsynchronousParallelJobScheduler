namespace ParallelJobScheduler
{
    public class ResponseModel
    {
        public string response { get; set; } = "";

        public ResponseModel() {  }

        public ResponseModel(string response)
        {
            this.response = response;
        }
    }
}

