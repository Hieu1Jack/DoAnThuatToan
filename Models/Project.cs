namespace GeneralizedKnapsackWeb.Models
{
    public class Project
    {
        public string Name { get; set; }
        public List<int> Constraints { get; set; } // Các ràng buộc như chi phí, nhân sự, v.v.
        public int Profit { get; set; }

        public Project()
        {
            Constraints = new List<int>();
        }
    }
}