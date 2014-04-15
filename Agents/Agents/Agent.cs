using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Agents
{
    public class Agent : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        public string _name, _phone, _image;

        public string Name { 
            get { return _name; } 
            set { _name = value; OnPropertyChanged("Name"); } 
        }
        public string Phone { 
            get { return _phone; } 
            set { _phone = value; OnPropertyChanged("Phone"); } 
        }
        public string Image { 
            get { return _image; } 
            set { _image = value; OnPropertyChanged("Image"); } 
        }

        public Agent(string name, string phone, string image)
        {
            this.Name = name;
            this.Phone = phone;
            this.Image = image;
            OnPropertyChanged("All");
        }

        public Agent AgentObject
        {
            get { return this; }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
