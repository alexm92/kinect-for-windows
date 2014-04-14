using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public class Agent
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Image { get; set; }

        public Agent(string name, string phone, string image)
        {
            this.Name = name;
            this.Phone = phone;
            this.Image = image;
        }
    }
}
