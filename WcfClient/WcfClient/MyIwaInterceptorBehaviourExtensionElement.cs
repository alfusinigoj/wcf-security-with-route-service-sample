using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;

namespace WcfClient
{
    public class MyIwaInterceptorBehaviourExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(MyIwaInterceptorEndpointBehaviour);

        protected override object CreateBehavior()
        {
            return new MyIwaInterceptorEndpointBehaviour();
        }
    }
}