using UnityEngine;
using System.Collections;


namespace TubeRendererExamples
{
	public class Herd : MonoBehaviour
	{
		const int CRITTER_COUNT = 20;
		const int POINT_COUNT = 50;
		const float POINT_SPACING = 0.05f;
		const float DISPERSION = 0.3f;

		Critter[] _critters;
		

		void Start()
		{
			// Create tiled texture.
			Texture2D texture = TubeRendererExamples.Helpers.CreateTileTexture( 12 );
			
			// Create critters.
			_critters = new Critter[ CRITTER_COUNT ];
			for( int s = 0; s < CRITTER_COUNT; s++ ){
				_critters[ s ] = new Critter();
				_critters[ s ].tube.transform.parent = gameObject.transform;
				_critters[ s ].tube.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
			}
		}
		
		
		void Update()
		{
			// Update all critters.
			foreach( Critter critter in _critters ) critter.Update();
		}
		
		
		class Critter
		{
			public TubeRenderer tube;
			
			
			public Critter()
			{
				// Create game object and add TubeRenderer component.
				tube = new GameObject( "Critter" ).AddComponent<TubeRenderer>();
				
				// Optimise for realtime manipulation.
				tube.MarkDynamic();
				
				// Define uv mapping for the end caps.
				tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
				
				// Define points and radiuses.
				tube.points = new Vector3[ POINT_COUNT ];
				tube.radiuses = new float[ POINT_COUNT ];
				for( int p = 0; p < POINT_COUNT; p++ ){
					tube.points[p] = SmoothRandom( - p * POINT_SPACING );
					tube.radiuses[p] = Mathf.Lerp( 0.2f, 0.05f, p/(POINT_COUNT-1f ) );
				}
			}
			
			
			public void Update()
			{
				// Calculate new position and store it in the beginning of the tube.
				tube.points[0] = SmoothRandom( Time.time * 0.2f );

				// Perform cheap inverse kinematics.
				for( int p=1; p<tube.points.Length; p++ ){
					Vector3 forward = tube.points[p] - tube.points[p-1];
					forward.Normalize();
					forward *= Herd.POINT_SPACING;
					tube.points[p] = tube.points[p-1] + forward;
				}

				// Overwrite point array reference to trigger mesh update.
				tube.points = tube.points;
			}
			
			
			// Cheap frequency modulation noise.
			Vector3 SmoothRandom( float t )
			{
				Random.InitState( tube.GetInstanceID() );
				float x = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.51f ) ) ) ) * 5;
				float y = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.78f ) ) ) ) * 3;
				float z = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.28f) ) ) ) * 5;
				return new Vector3( x, y, z );
			}
		}
	}
}