using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCT
{
    public enum EventType
    {
        addnewitem,
        updateitem,
        itemclick,
        deleteitem,
        clearitemslist,
        opennewitemdialog,
        openedititemdialog,
        tagclick,        
        deletetag,
        cleartagslist,
        addnewtag,
        updateitemcount,
        updatetagcount,
        clearclipboardmemory,
        addlogitemtext,
        clearitemvalue,
        savenewitem,
        savemodifieditem,
        close_app_remotely,
        networkmessage
    }
       
    public class CustomEvents
    {
        public event EventHandler<CustomEventArgs> ApplicationEvent;
        
        public void AppEvent(CustomEventArgs e)
        {
            EventHandler<CustomEventArgs> handler = ApplicationEvent;

            if(handler != null)
            {
                handler(null, e);
            }
        }      
    }

    public class CustomEvent
    {

    }

    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs(object tempmessage, EventType tempeventtype)
        {            
            message = tempmessage;
            et = tempeventtype;
        }

        public object message = "";
        public EventType et;
    }

}
