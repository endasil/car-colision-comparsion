using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class ImpulseMeshDeformer : MonoBehaviour
    {
        private float _deformScalar = 0.1f;

        // Only deform vertices that are within this distance from the collision
        private float maxVertexCollisionPointDistance = 0.5f;
        // List of all mesh filters who's mesh we want to deform
        [SerializeField] private List<MeshFilter> _meshFilters;

        [SerializeField] private float _minimumImpulseToCauseDeformation = 50f;
        // List of vertices in the mesh as it was before applying damage 
        private List<Vector3[]> originalMeshData;

        // List of vertices in the damaged mesh
        List<Vector3[]> damagedMeshData;

        // Preallocate memory to hold contact points to reduce the need for memory allocation and garbage at runtime
        private readonly List<ContactPoint> _collisionContacts = new List<ContactPoint>(50);

        // Only allow a vertice to be moved this far from its original point
        private float _maxDeformOffset = 0.5f;
        
        void Start()
        {
            if (_meshFilters.Count == 0)
                _meshFilters = GetComponentsInChildren<MeshFilter>(true).ToList();

            // Initialize a list of original

            originalMeshData = new List<Vector3[]>(_meshFilters.Count);
            damagedMeshData = new List<Vector3[]>(_meshFilters.Count);

            // We need a original list of vertices for each mesh filter as well as a damaged list
            // Loop trough the mesh filters and copy their vertice data to our damaged and original mesh struct / class
            foreach (var meshFilter in _meshFilters)
            {
                originalMeshData.Add(meshFilter.mesh.vertices);
                damagedMeshData.Add(meshFilter.mesh.vertices);
            }
        }
        public void OnCollisionEnter(Collision collision) => DeformMeshes(collision);

        void DeformMeshes(Collision collision)
        {
            int nrOfContactPoints = collision.GetContacts(_collisionContacts);
            if (collision.impulse.magnitude < _minimumImpulseToCauseDeformation)
                return;

            Debug.Log("Impulse mesh magnitude: " + collision.impulse.magnitude);
            // Loop trough the list of all mesh filters in the vehicle.
            for (var meshNr = 0; meshNr < _meshFilters.Count; meshNr++)
            {
                var meshFilter = _meshFilters[meshNr];
                // Skip inactive / empty mesh filters
                if (meshFilter.mesh != null && !meshFilter.gameObject.activeSelf)
                    continue;

                // Loop trough all contact points of the collision.
                for (var index = 0; index < nrOfContactPoints; index++)
                {
                    var collisionContact = _collisionContacts[index];

                    // Calculate the collision direction in local coordinate system of the car. 
                    Vector3 localDirection = meshFilter.transform.InverseTransformDirection(collisionContact.normal);

                    // Change the collision point to local coordinates of the meshs object.
                    Vector3 localContactPoint = meshFilter.transform.InverseTransformPoint(collisionContact.point);

                    // Calculate the amount meshes should be deformed by taking the local collision direction and scaling
                    // it with the impulse and then multiplying it with scaling modifier.
                    var deformVector = localDirection * _deformScalar * collision.impulse.magnitude;
                    
                    // Go trough each vertices in the meshFilters mesh.
                    for (int vertexNr = 0; vertexNr < damagedMeshData[meshNr].Length; vertexNr++)
                    {
                        // If distance between the collision point and the vertex is too far, ignore
                        // this vertex and move on to the next one.
                        if ((localContactPoint - damagedMeshData[meshNr][vertexNr]).magnitude > maxVertexCollisionPointDistance) 
                            continue;

                        // Modify the vertices of the mesh with the deformVector calculated for this collision point.
                        damagedMeshData[meshNr][vertexNr] += deformVector;

                        // Calculate the distance this vertex has moved compared to when the mesh was undamaged.
                        // If distance to original position is greater than the distance limit
                        Vector3 originalToDamageVertexVector =
                            damagedMeshData[meshNr][vertexNr] - originalMeshData[meshNr][vertexNr];

                        
                        var directionToDamagedVertex = originalToDamageVertexVector.normalized;
                        var distanceToOriginalPoint = originalToDamageVertexVector.magnitude;
                        if (distanceToOriginalPoint > _maxDeformOffset)
                        {
                            // Clamp deform offset to _meshDeformOffset distance
                            damagedMeshData[meshNr][vertexNr] = originalMeshData[meshNr][vertexNr] +
                                                                directionToDamagedVertex * _maxDeformOffset;
                        }
                    }
                }
                
                // Update the mesh with the new vertices. Recalculate normals as well so light is reflected correctly.
                _meshFilters[meshNr].mesh.SetVertices(damagedMeshData[meshNr]);
                _meshFilters[meshNr].mesh.RecalculateNormals();
            }
        }
    }
}
