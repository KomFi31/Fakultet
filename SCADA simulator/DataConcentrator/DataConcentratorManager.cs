/*
 * Predstavlja centralnu logiku Data Concentrator komponente.
 * Upravlja tagovima i predstavlja vezu izmedju baze podataka,
 * PLC simulatora i SCADA korisnickog interfejsa.
 */

using DataConcentrator.Model;
using System.Collections.Generic;
using System.Linq;

namespace DataConcentrator
{
    public class DataConcentratorManager
    {
        private static DataConcentratorManager instance;

        public static DataConcentratorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataConcentratorManager();
                }

                return instance;
            }
        }

        private DataConcentratorManager()
        {
        }

        public List<Tag> GetAllTags()
        {
            return ContextClass.Instance.Tags.ToList();
        }

        public Tag GetTag(string name)
        {
            return ContextClass.Instance.Tags
                .FirstOrDefault(tag => tag.Name == name);
        }

        public bool AddTag(Tag tag)
        {
            if (tag == null)
                return false;

            if (GetTag(tag.Name) != null)
                return false;

            ContextClass.Instance.Tags.Add(tag);
            ContextClass.Instance.SaveChanges();

            return true;
        }

        public bool RemoveTag(string name)
        {
            Tag tag = GetTag(name);

            if (tag == null)
                return false;

            ContextClass.Instance.Tags.Remove(tag);
            ContextClass.Instance.SaveChanges();

            return true;
        }
    }
}