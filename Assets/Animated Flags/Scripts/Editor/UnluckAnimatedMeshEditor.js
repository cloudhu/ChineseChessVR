//	Unluck Software	
// 	www.chemicalbliss.com
@CustomEditor(UnluckAnimatedMesh)
@CanEditMultipleObjects
public class UnluckAnimatedMeshEditor extends Editor {	
    override function OnInspectorGUI() {    	
        DrawDefaultInspector();
		
		if(GUILayout.Button("Force Change Mesh")){
			target.FillCacheArray();
		}
        if (GUI.changed){ 
	        target.CheckIfMeshHasChanged();
	        EditorUtility.SetDirty(target);
        }
    }
}