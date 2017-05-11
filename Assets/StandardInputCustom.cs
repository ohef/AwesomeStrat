using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StandardInputCustom : StandaloneInputModule {
    public override void Process()
    {
        bool flag = this.SendUpdateEventToSelectedObject();
        if ( base.eventSystem.sendNavigationEvents )
        {
            if ( !flag )
            {
                flag |= this.SendMoveEventToSelectedObject();
            }
            if ( !flag )
            {
                this.SendSubmitEventToSelectedObject();
            }
        }
        //if ( !this.ProcessTouchEvents() && base.input.mousePresent )
        //{
        //    this.ProcessMouseEvent();
        //}
    }
}
