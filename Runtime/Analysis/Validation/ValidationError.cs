//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    public class ValidationError
    {
        public UnityEngine.Object AffectedObject { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}

#endif
