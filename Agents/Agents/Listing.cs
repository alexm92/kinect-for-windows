using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Agents
{
    public class Listing : INotifyPropertyChanged
    {
        #region Variables
        /// <summary>
        /// Declare the event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private string _title, _description, _address, _map, _price, _type, _bedrooms, _bathrooms, _size, _maintenance, _agent;
        private string[] _images, _amenities, _subway;
        #endregion

        #region Setters and Getters

        public string Title {
            get { return _title; }
            set { _title = value; OnPropertyChanged("Title"); } 
        }
        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }
        public string Address
        {
            get { return _address; }
            set { _address = value; OnPropertyChanged("Address"); }
        }
        public string Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged("Map"); }
        }
        public string Price
        {
            get { return _price; }
            set { _price = value; OnPropertyChanged("Price"); }
        }
        public string Type
        {
            get { return _type; }
            set { _type = value; OnPropertyChanged("Type"); }
        }
        public string Bedrooms
        {
            get { return _bedrooms; }
            set { _bedrooms = value; OnPropertyChanged("Bedrooms"); }
        }
        public string Bathrooms
        {
            get { return _bathrooms; }
            set { _bathrooms = value; OnPropertyChanged("Bathrooms"); }
        }
        public string Size
        {
            get { return _size; }
            set { _size = value; OnPropertyChanged("Size"); }
        }
        public string Maintenance
        {
            get { return _maintenance; }
            set { _maintenance = value; OnPropertyChanged("Maintenance"); }
        }
        public string Agent
        {
            get { return _agent; }
            set { _agent = value; OnPropertyChanged("Agent"); }
        }        

        public string[] Images
        {
            get { return _images; }
            set { _images = value; OnPropertyChanged("Images"); }
        }
        public string[] Amenities
        {
            get { return _amenities; }
            set { _amenities = value; OnPropertyChanged("Amenities"); }
        }
        public string[] Subway
        {
            get { return _subway; }
            set { _subway = value; OnPropertyChanged("Subway"); }
        }


        /// <summary>
        /// Returns the current object. Used in bindings
        /// </summary>
        public Listing ListingObject
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
