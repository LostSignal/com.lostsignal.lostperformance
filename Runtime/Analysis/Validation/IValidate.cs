//-----------------------------------------------------------------------
// <copyright file="IValidate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;

    public interface IValidate
    {
        void Validate(List<ValidationError> errors);
    }
}

#endif
