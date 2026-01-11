using System;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;

public class Interactable : GameObject
{
    protected TriggerHitbox triggerHitbox;
    private Rectangle _interactionArea;
    private HitboxLayer _detectsLayers;
    private HitboxManager _hitboxManager;

    public Interactable(Rectangle interactionArea, HitboxManager hitboxManager, HitboxLayer detectsLayers = HitboxLayer.Player)
    {
        _interactionArea = interactionArea;
        _detectsLayers = detectsLayers;

        triggerHitbox = new TriggerHitbox(_interactionArea);
        
        triggerHitbox.DetectsLayers = _detectsLayers;

        triggerHitbox.OnStay += OnTriggerStay;

        triggerHitbox.OnExit += OnTriggerExit;

        _hitboxManager = hitboxManager;
        _hitboxManager.AddTrigger(triggerHitbox);
    }
    private void OnTriggerExit(TriggerHitbox trigger, object obj, TriggerSide side)
    {
        if (obj is Player player)
        {
            if (player.ClosestInteractable == this)
            {
                if (this is NPC npc)
                {
                    npc.speechBubble.IsVisible = false;
                }
                player.ClosestInteractable = null;
                player.ClosestInteractableDist = int.MaxValue;
            }
        }
    }

    private void OnTriggerStay(TriggerHitbox trigger, object obj)
    {
        if (obj is Player player)
        {
            Vector2 centerOfArea = new Vector2(_interactionArea.X + _interactionArea.Width / 2, _interactionArea.Y + _interactionArea.Height / 2);
            int distance = (int)Vector2.Distance(player.GlobalPosition + player.CameraOffset, centerOfArea);

            if (distance < player.ClosestInteractableDist || player.ClosestInteractable == this)
            {
                if (player.ClosestInteractable != this && player.ClosestInteractable is NPC npc)
                {
                    npc.speechBubble.IsVisible = false;
                }
                player.ClosestInteractable = this;
                player.ClosestInteractableDist = distance;
            }
        }
    }
}