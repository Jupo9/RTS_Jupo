using System;
using System.Collections.Generic;
using System.Text;

public interface ISelectable
{
    bool IsSelected { get; }
    void Select();
    void Deselect();
}
