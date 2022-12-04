using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
	public List<Material> _spawnableMaterials = new List<Material>();

	public GameObject _objectPrefab;
	public Transform _spawnPosition;

    public void SpawnObject()
	{
		Debug.Log("Spawning Cube");

		GameObject spawnedObject = Instantiate(_objectPrefab, _spawnPosition);
		SetRandomMaterial(spawnedObject);

		Destroy(spawnedObject, 10);
	}

	void SetRandomMaterial(GameObject newObject)
	{
		var allMeshes = newObject.GetComponentsInChildren<Renderer>();

		if (allMeshes.Length == 0)
		{
			Debug.LogError("No Meshes found on object!", this);
			return;
		}

		int listCount = _spawnableMaterials.Count;

        if (listCount == 0)
        {
            Debug.LogError("No Materials to choose from!", this);
            return;
        }

        int randIndex = Random.Range(0, listCount);
		var randomMat = _spawnableMaterials[randIndex];

		for (int i = 0; i < allMeshes.Length; i++)
		{
			var mesh = allMeshes[i];
			mesh.material = randomMat;
		}
	}
}
