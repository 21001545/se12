using System.Collections;
using System.Collections.Generic;
using LunarConsolePlugin;
[CVarContainer]

public static class CheatVariables
{
    // 각 치트가 사용될 영역을 기준으로, region을 나눠 둡시다!
    // 이름은 어떻게 할지 고민을 좀 더..
    #region World Tree
    public static readonly CVar tree_showDebug = new CVar("tree_showDebug", false);
    #endregion
}
