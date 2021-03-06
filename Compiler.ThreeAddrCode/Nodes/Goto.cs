﻿using System;

namespace Compiler.ThreeAddrCode.Nodes
{
    /// <summary>
    ///     Оператор безусловного перехода
    /// </summary>
    public class Goto : Node
    {
        /// <summary>
        ///     Метка-идентификатор оператора, на который происходит переход
        /// </summary>
        public Guid TargetLabel { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : goto {1}", Label, TargetLabel);
        }
    }
}