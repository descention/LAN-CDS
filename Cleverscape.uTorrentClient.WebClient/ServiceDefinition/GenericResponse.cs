﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Cleverscape.UTorrentClient.WebClient.ServiceDefinition
{

    [DataContract(Namespace = "")]
    public class GenericResponse
    {
        [DataMember(Name = "build", Order = 1)]
        public int BuildNumber { get; set; }

    }
}
