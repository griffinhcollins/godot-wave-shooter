
// Applies to anything that can be a prerequisite to unlocking another upgrade. Currently used only for the upgrade tree
using System.Collections.Generic;
using Godot;

public interface Prerequisite
{

    List<Prerequisite> GetPrerequisites(Condition condition = null);

    string GetName();

    string GetIconName();
}