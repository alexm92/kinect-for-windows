using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Agents
{
    public class Agent : INotifyPropertyChanged
    {
        #region Variables
        /// <summary>
        /// Declare the event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name, _phone, _image, _email, _license, _description, _university;
        private string[] _neighbourhoods, _languages;
        #endregion

        #region Setters and Getters
        /// <summary>
        /// Name of the agent
        /// </summary>
        public string Name { 
            get { return _name; } 
            set { _name = value; OnPropertyChanged("Name"); } 
        }

        /// <summary>
        /// Phone Number
        /// </summary>
        public string Phone { 
            get { return _phone; } 
            set { _phone = value; OnPropertyChanged("Phone"); } 
        }

        /// <summary>
        /// Image
        /// </summary>
        public string Image { 
            get { return _image; } 
            set { _image = value; OnPropertyChanged("Image"); } 
        }

        /// <summary>
        /// Email
        /// </summary>
        public string Email
        {
            get { return _email; }
            set { _email = value; OnPropertyChanged("Email"); }
        }

        /// <summary>
        /// License
        /// </summary>
        public string License
        {
            get { return _license; }
            set { _license = value; OnPropertyChanged("License"); }
        }

        /// <summary>
        /// Personal description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }

        /// <summary>
        /// Graduated from 
        /// </summary>
        public string University
        {
            get { return _university; }
            set { _university = value; OnPropertyChanged("University"); }
        }

        /// <summary>
        /// List of neighbourhoods
        /// </summary>
        public string[] Neighbourhoods
        {
            get { return _neighbourhoods; }
            set { _neighbourhoods = value; OnPropertyChanged("Neighbourhoods"); }
        }

        /// <summary>
        /// List of languages known by this agent
        /// </summary>
        public string[] Languages
        {
            get { return _languages; }
            set { _languages = value; OnPropertyChanged("Languages"); }
        }


        /// <summary>
        /// Returns the current object. Used in bindings
        /// </summary>
        public Agent AgentObject
        {
            get { return this; }
        }
        #endregion

        /// <summary>
        /// Create the OnPropertyChanged method to raise the event
        /// </summary>
        /// <param name="name">Propery that is changing</param>
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
