﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agents
{
    public interface IDaemonFactory
    {
        IDaemon BuildDaemon();
    }
}