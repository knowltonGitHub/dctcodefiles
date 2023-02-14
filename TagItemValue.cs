using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCT.Classes
{
    public class TagItemValue
    {
        public TagItemValue() { }
        

        public TagItemValue(string tag = null, string item = null, string value = null, string userid = null) 
        {
            _tag = tag;
            _item = item;
            _value = value;
            _userid = userid;
        }

        public string _tag;
        public string _item;
        public string _value;
        public string _userid;
    }
    public class TagItemValueManager
    {
        public List<TagItemValue> _tivList;
        public TagItemValueManager()
        {
            _tivList = new List<TagItemValue>() { };
        }

        public bool RemoveItem(string tag, string item, string userid)
        {
            bool removed = false;             
            TagItemValue temptiv =_tivList.Find(x => x._tag == tag && x._item == item && x._userid == userid);
            removed = _tivList.Remove(temptiv);
            return removed;
        }

        //maybe implement this?
        //public bool RemoveItem(TagItemValue tivtemp)
        //{
        //    bool removed = false;
        //    TagItemValue temptiv = _tivList.Find(x => x._tag == tag && x._item == item && x._userid == userid);
        //    removed = _tivList.Remove(temptiv);
        //    return removed;
        //}

        public bool RemoveTag(string tag, string userid)
        {
            bool removed = false;
            //TagItemValue temptiv = new TagItemValue(tag, item, GetItemValue(tag, item, userid), userid);

            TagItemValue temptiv = _tivList.Find(x => x._tag == tag && x._userid == userid);
            removed = _tivList.Remove(temptiv);
            return removed;
        }
        public string GetItemValue(string temptag, string tempitem, string userid)
        {
            return _tivList.Where(x => x._tag == temptag && x._item == tempitem && x._userid == userid).SingleOrDefault()._value;
        }

        public List<string> GetItems(string temptag, string userid)
        {
            List<string> itemsList = new List<string>() { };

            foreach (TagItemValue tiv in (List<TagItemValue>)_tivList)
            {
                if ((tiv._tag == temptag) && (tiv._userid == userid))
                {
                    if ((tiv._item != "") && (tiv._item != null))
                    {
                        itemsList.Add(tiv._item);
                    }
                }
            }

            return itemsList;

        }

        public List<string> GetTags(string userid)
        {
            List<string> tagsList = new List<string>() { };

            foreach (string tiv in _tivList.Select(x => x._tag).Distinct())
            {
                tagsList.Add(tiv);
            }

            return tagsList;
        }


        public int GetTagItemValueCOUNT
        {
            get { return _tivList.Count; }
        }      

        public int SetOwnerIDForAllTagItemValues
        {
            set
            {
                foreach (TagItemValue tiv in _tivList)
                {
                    tiv._userid = value.ToString();
                }
            }
        }

    }//end class
}//end namespace
