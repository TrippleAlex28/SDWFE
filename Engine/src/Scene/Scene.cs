using Engine.Network.Shared.Object;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Scene;

public abstract class Scene
{
     public string Name { get; private set; }
     
     public Type? DefaultPlayerClass { get; private set; }
     public Func<GameObject>? DefaultPlayerClassFactory { get; private set; }
     public Vector2 SpawnPoint { get; protected set; } = Vector2.Zero;

     public Color BackgroundColor { get; protected set; } = Color.Black;

     public GameObject SceneRoot { get; private set; }
     private int _nextNetworkId = 1;
     
     public UIRoot UIRoot { get; private set; }
     
     public Scene(string name)
     {
          Name = name;
          SceneRoot = new GameObject();
          UIRoot = new UIRoot();
     }

     protected void SetDefaultPlayerClass<T>(Func<T> factory) where T : GameObject
     {
          DefaultPlayerClass = typeof(T);
          DefaultPlayerClassFactory = factory;
     }

     protected void SetDefaultPlayerClassNull()
     {
          DefaultPlayerClass = null;
          DefaultPlayerClassFactory = null;
     }
     
     public virtual void Update(GameTime gameTime)
     {
          UIRoot.layoutSlot = new Rectangle(0, 0, ExtendedGame.DrawResolution.X, ExtendedGame.DrawResolution.Y);
          
          SceneRoot.Update(gameTime);
          UIRoot.Update(gameTime);
     }

     public virtual void DrawScene(SpriteBatch spriteBatch)
     {
          SceneRoot.Draw(spriteBatch);
     }

     public virtual void DrawUI(SpriteBatch spriteBatch)
     {
          UIRoot.Draw(spriteBatch);
     }

     /// <summary>
     /// Startup Logic
     /// </summary>
     public virtual void Enter()
     {
          
     }

     /// <summary>
     /// Cleanup Logic
     /// </summary>
     public virtual void Exit()
     {
          
     }

     /// <summary>
     /// Update the dirty flags for all NetObjects in the scene
     /// </summary>
     public void UpdateDirty()
     {
          UpdateDirtyRecursive(SceneRoot);
     }

     /// <summary>
     /// Clear the dirty flags for all NetObjects in the scene
     /// </summary>
     public void ClearDirty()
     {
          ClearDirtyRecursive(SceneRoot);
     }
     
     /// <summary>
     /// Register all world objects to prepare them for replication
     /// </summary>
     public void RegisterExistingWorldObjects()
     {
          RegisterRecursive(SceneRoot);
     }

     /// <summary>
     /// Create the default player class and manage it
     /// </summary>
     public void AddPlayer(int owningClientId)
     {
          if (DefaultPlayerClassFactory == null)
          {
               Console.WriteLine($"Scene.AddPlayer: No default player class factory");
               return;
          }

          if (GetPawn(owningClientId) != null)
          {
               Console.WriteLine($"Scene.AddPlayer: This client already owns a player");
               return;
          }
          
          var player = DefaultPlayerClassFactory();
          AddObject(
               player,
               owningClientId: owningClientId
          );
     }

     /// <summary>
     /// Remove all objects owned by this client
     /// </summary>
     public void RemoveClientObjects(int clientId)
     {
          List<GameObject> objects = GetClientObjectsRecursive(SceneRoot, clientId, true);
          for (int i = objects.Count - 1; i >= 0; --i)
          {
               objects[i].RemoveFromParent();
          }
     }
     
     /// <summary>
     /// Add a GameObject to the current scene
     /// </summary>
     public void AddObject(GameObject obj, GameObject? sceneParent = null, int owningClientId = -1)
     {
          // Register for replication
          RegisterRecursive(obj, owningClientId);

          // Add to the scene
          if (sceneParent != null)
               sceneParent.AddChild(obj);
          else
               SceneRoot.AddChild(obj);
     }

     /// <summary>
     /// Remove an object with a certain id from the current scene
     /// </summary>
     public void RemoveObject(int netId)
     {
          GameObject? obj = GetObject(netId);
          obj?.RemoveFromParent();
     }

     /// <summary>
     /// Gets client pawn
     /// </summary>
     public GameObject? GetPawn(int clientId)
     {
          return GetPawnRecursive(SceneRoot, clientId);
     }

     /// <summary>
     /// Get GameObject with a specific network ID
     /// </summary>
     public GameObject? GetObject(int netId)
     {
          return GetObjectRecursive(SceneRoot, netId);
     }
     
     #region Helpers
     
     private void RegisterRecursive(GameObject node, int owningClientId = -1)
     {
          if (node.ReplicatesOverNetwork)
               RegisterObject(node, owningClientId);

          foreach (GameObject child in node.Children)
               RegisterRecursive(child, owningClientId);
     }
     
     private void RegisterObject(GameObject obj, int owningClientId = -1)
     {
          if (obj.NetworkId == -1)
               obj.NetworkId = _nextNetworkId++;

          obj.OwningClientId = owningClientId;
     }
     
     private GameObject? GetPawnRecursive(GameObject node, int clientId)
     {
          if (node.OwningClientId == clientId && node.GetType() == DefaultPlayerClass)
               return node;

          foreach (var child in node.Children)
          {
               var found = GetPawnRecursive(child, clientId);
               if (found != null)
                    return found;
          }

          return null;
     }

     private List<GameObject> GetClientObjectsRecursive(GameObject node, int clientId, bool skipNode = false)
     {
          List<GameObject> objects = [];

          if (!skipNode)
          {
               if (node.OwningClientId == clientId)
                    objects.Add(node);
          }
          
          foreach (var c in node.Children)
          {
               List<GameObject> found = GetClientObjectsRecursive(c, clientId);
               objects.AddRange(found);
          }

          return objects;
     }
     
     private GameObject? GetObjectRecursive(GameObject node, int netId)
     {
          if (node.NetworkId == netId)
               return node;
          
          foreach (var child in node.Children)
          {
               GameObject? found = GetObjectRecursive(child, netId);
               if (found != null)
                    return found;
          }

          return null;
     }
     
     private void UpdateDirtyRecursive(GameObject node)
     {
          node.UpdateDirty();
          
          foreach (GameObject child in node.Children)
               child.UpdateDirty();
     }
     
     private void ClearDirtyRecursive(GameObject node)
     {
          node.ClearDirty();
          
          foreach (GameObject child in node.Children)
               child.ClearDirty();
     }
     
     #endregion
     
     #region Dumping Scene
     
     public void WriteScene()
     {
          Console.WriteLine("===== START SCENE");
          WriteRecursive(SceneRoot);
          Console.WriteLine("===== END SCENE + START UI");
          WriteRecursive(UIRoot);
          Console.WriteLine("===== END UI");
     }
    
     private void WriteRecursive(GameObject node, int indent = 0)
     {
          for (int i = 0; i < indent; i++)
          {
               Console.Write("  ");
          }
          Console.WriteLine(node.GetType().Name);

          foreach (var c in node.Children)
               WriteRecursive(c, indent + 1);
     }
     
     #endregion 
}