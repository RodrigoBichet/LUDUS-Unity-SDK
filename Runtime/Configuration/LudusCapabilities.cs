using System;
using UnityEngine;

namespace LudusSDK
{
    [Serializable]
    public sealed class LudusCapabilities
    {
        [Header("Interações automáticas")]

        [Tooltip("Registra cliques ou toques na tela.")]
        public bool clicks = true;

        [Tooltip("Registra o caminho do mouse ou toque.")]
        public bool mousePath = true;

        [Tooltip("Registra trajetórias de arraste quando disponíveis.")]
        public bool dragPath = false;

        [Tooltip("Permite capturas de imagem da sessão.")]
        public bool screenshots = false;

        [Tooltip("Registra períodos de inatividade.")]
        public bool inactivity = false;

        [Tooltip("Registra ganho ou perda de foco quando disponível.")]
        public bool focusEvents = false;

        [Header("Eventos informados pelo jogo")]

        [Tooltip("O jogo informa início ou conclusão de fases.")]
        public bool phaseEvents = false;

        [Tooltip("O jogo informa acertos e erros.")]
        public bool correctWrong = false;

        [Tooltip("O jogo informa categorias ou módulos.")]
        public bool categoryEvents = false;

        [Tooltip("O jogo pode registrar eventos semânticos próprios.")]
        public bool customEvents = true;

        public LudusCapabilities Clone()
        {
            return new LudusCapabilities
            {
                clicks = clicks,
                mousePath = mousePath,
                dragPath = dragPath,
                screenshots = screenshots,
                inactivity = inactivity,
                focusEvents = focusEvents,
                phaseEvents = phaseEvents,
                correctWrong = correctWrong,
                categoryEvents = categoryEvents,
                customEvents = customEvents,
            };
        }
    }
}
