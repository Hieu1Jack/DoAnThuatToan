using Microsoft.AspNetCore.Mvc;
using GeneralizedKnapsackWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace GeneralizedKnapsackWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate(int constraintCount, int projectCount, List<int> limits, List<Project> projects)
        {
            // Sắp xếp dự án theo heuristic: profit / tổng các ràng buộc
            projects.Sort((a, b) => (b.Profit / (double)b.Constraints.Sum())
                .CompareTo(a.Profit / (double)a.Constraints.Sum()));

            Queue<Node> queue = new Queue<Node>();
            Node root = new Node(constraintCount)
            {
                Level = -1,
                Profit = 0,
                Bound = 0
            };
            queue.Enqueue(root);

            int maxProfit = 0;
            List<string> bestSelection = new List<string>();

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                if (current.Level == -1) current.Level = 0;

                if (current.Level >= projects.Count) continue;

                Project currentProject = projects[current.Level];

                // Với dự án
                Node with = new Node(constraintCount)
                {
                    Level = current.Level + 1,
                    Profit = current.Profit + currentProject.Profit,
                    SelectedProjects = new List<string>(current.SelectedProjects)
                };
                for (int i = 0; i < constraintCount; i++)
                {
                    with.TotalConstraints[i] = current.TotalConstraints[i] + currentProject.Constraints[i];
                }
                with.SelectedProjects.Add(currentProject.Name);

                if (IsWithinLimits(with.TotalConstraints, limits))
                {
                    if (with.Profit > maxProfit)
                    {
                        maxProfit = with.Profit;
                        bestSelection = with.SelectedProjects;
                    }

                    with.Bound = CalculateBound(with, projects, limits);
                    if (with.Bound > maxProfit)
                        queue.Enqueue(with);
                }

                // Không chọn
                Node without = new Node(constraintCount)
                {
                    Level = current.Level + 1,
                    Profit = current.Profit,
                    TotalConstraints = new List<int>(current.TotalConstraints),
                    SelectedProjects = new List<string>(current.SelectedProjects),
                    Bound = CalculateBound(current, projects, limits)
                };

                if (without.Bound > maxProfit)
                    queue.Enqueue(without);
            }

            ViewBag.Result = new
            {
                SelectedProjects = bestSelection,
                Constraints = limits.Select((limit, i) => new
                {
                    Index = i + 1,
                    Total = bestSelection.Sum(name => projects.Find(p => p.Name == name).Constraints[i]),
                    Limit = limit
                }),
                MaxProfit = maxProfit
            };

            return View("Index");
        }

        private double CalculateBound(Node node, List<Project> projects, List<int> limits)
        {
            double bound = node.Profit;
            List<int> total = new List<int>(node.TotalConstraints);
            int j = node.Level;

            while (j < projects.Count && CanFit(total, projects[j].Constraints, limits))
            {
                for (int i = 0; i < limits.Count; i++)
                    total[i] += projects[j].Constraints[i];
                bound += projects[j].Profit;
                j++;
            }

            if (j < projects.Count)
            {
                double ratio = 1.0;
                for (int i = 0; i < limits.Count; i++)
                {
                    int remaining = limits[i] - total[i];
                    if (projects[j].Constraints[i] == 0) continue;
                    ratio = Math.Min(ratio, remaining / (double)projects[j].Constraints[i]);
                }
                bound += projects[j].Profit * ratio;
            }

            return bound;
        }

        private bool IsWithinLimits(List<int> totals, List<int> limits)
        {
            for (int i = 0; i < limits.Count; i++)
                if (totals[i] > limits[i]) return false;
            return true;
        }

        private bool CanFit(List<int> totals, List<int> add, List<int> limits)
        {
            for (int i = 0; i < limits.Count; i++)
                if (totals[i] + add[i] > limits[i]) return false;
            return true;
        }
    }
}