namespace GeneralizedKnapsackWeb.Models
{
    public class Node
    {
        public int Level { get; set; }
        public List<int> TotalConstraints { get; set; }
        public int Profit { get; set; }
        public double Bound { get; set; }
        public List<string> SelectedProjects { get; set; }

        public Node(int constraintCount)
        {
            TotalConstraints = new List<int>(new int[constraintCount]);
            SelectedProjects = new List<string>();
        }
    }
}