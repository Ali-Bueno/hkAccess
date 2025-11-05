using HarmonyLib;
using TMPro;

namespace HKAccessibility.Patches
{
    /// <summary>
    /// Patches for in-game dialogue boxes
    /// Announces NPC conversations and dialogue text
    /// </summary>
    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.ShowPage))]
    public class DialogueBox_ShowPage_Patch
    {
        private static void Postfix(DialogueBox __instance, int pageNum)
        {
            try
            {
                var textMesh = Traverse.Create(__instance).Field("textMesh").GetValue<TextMeshPro>();
                if (textMesh != null && textMesh.textInfo != null)
                {
                    // Extract the text for the current page
                    int pageIndex = pageNum - 1;
                    if (pageIndex >= 0 && pageIndex < textMesh.textInfo.pageCount)
                    {
                        TMP_PageInfo pageInfo = textMesh.textInfo.pageInfo[pageIndex];
                        string fullText = textMesh.text;

                        if (pageInfo.firstCharacterIndex >= 0 &&
                            pageInfo.lastCharacterIndex < fullText.Length &&
                            pageInfo.lastCharacterIndex >= pageInfo.firstCharacterIndex)
                        {
                            string pageText = fullText.Substring(
                                pageInfo.firstCharacterIndex,
                                pageInfo.lastCharacterIndex - pageInfo.firstCharacterIndex + 1
                            );

                            pageText = pageText.Replace("<br>", " ").Trim();

                            if (!string.IsNullOrEmpty(pageText))
                            {
                                Plugin.Logger.LogInfo($"[DialogueBox_ShowPage] Speaking: {pageText}");
                                TolkScreenReader.Instance.Speak(pageText, false);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Logger.LogError($"Error in DialogueBox_ShowPage_Patch: {ex}");
            }
        }
    }
}
