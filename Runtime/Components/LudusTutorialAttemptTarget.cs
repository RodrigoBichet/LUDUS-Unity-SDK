using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LudusSDK
{
    public enum LudusTutorialAttemptMode
    {
        UiDrop,
        Physics2D,
    }

    /// <summary>
    /// Controla somente o feedback das tentativas feitas no laboratório
    /// tutorial. Não registra resultados semânticos e não deve ser usado no
    /// jogo real.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class LudusTutorialAttemptTarget :
        MonoBehaviour,
        IDropHandler
    {
        [SerializeField]
        private LudusTutorialAttemptMode mode;

        [SerializeField]
        private string expectedTag = "Untagged";

        [SerializeField]
        private Text feedbackLabel;

        [SerializeField]
        private LudusTutorialDraggableItem[] pieces =
            Array.Empty<LudusTutorialDraggableItem>();

        public event Action<bool> AttemptEvaluated;

        public void Configure(
            LudusTutorialAttemptMode attemptMode,
            string correctTag,
            Text feedback,
            LudusTutorialDraggableItem[] draggablePieces
        )
        {
            mode = attemptMode;
            expectedTag = string.IsNullOrWhiteSpace(correctTag)
                ? "Untagged"
                : correctTag;
            feedbackLabel = feedback;
            pieces = draggablePieces ?? Array.Empty<LudusTutorialDraggableItem>();
            SetFeedback("Faça uma tentativa para continuar.", false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (mode != LudusTutorialAttemptMode.UiDrop)
            {
                return;
            }

            EvaluateAttempt(eventData?.pointerDrag);
        }

        public bool EvaluateAttempt(GameObject candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            bool correct = candidate.CompareTag(expectedTag);
            SetFeedback(
                correct
                    ? "Resposta correta. Você já pode avançar."
                    : "Essa opção não corresponde ao destino. Tente novamente.",
                correct
            );
            ResolvePiecesAfterAttempt(candidate, correct);
            AttemptEvaluated?.Invoke(correct);
            return correct;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (mode == LudusTutorialAttemptMode.Physics2D)
            {
                EvaluateAttempt(other == null ? null : other.gameObject);
            }
        }

        private void ReturnAllPieces()
        {
            foreach (LudusTutorialDraggableItem piece in pieces)
            {
                piece?.ReturnToInitialPosition();
            }
        }

        private void ResolvePiecesAfterAttempt(
            GameObject candidate,
            bool correct
        )
        {
            if (!correct)
            {
                ReturnAllPieces();
                return;
            }

            LudusTutorialDraggableItem correctPiece =
                candidate.GetComponentInParent<
                    LudusTutorialDraggableItem
                >();
            if (correctPiece == null)
            {
                ReturnAllPieces();
                return;
            }

            foreach (LudusTutorialDraggableItem piece in pieces)
            {
                if (piece == null || piece == correctPiece)
                {
                    continue;
                }

                piece.ReturnToInitialPosition();
            }

            RectTransform pieceRect = correctPiece.transform as RectTransform;
            RectTransform targetRect = transform as RectTransform;
            if (pieceRect != null && targetRect != null)
            {
                pieceRect.position = targetRect.position;
            }

            correctPiece.SetInteractionEnabled(false);
        }

        private void SetFeedback(string message, bool correct)
        {
            if (feedbackLabel == null)
            {
                return;
            }

            feedbackLabel.text = message;
            feedbackLabel.color = correct
                ? new Color(0.52f, 0.86f, 0.76f)
                : new Color(0.92f, 0.72f, 0.38f);
        }
    }
}
