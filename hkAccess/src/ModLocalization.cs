using System.Collections.Generic;

namespace HKAccessibility
{
    /// <summary>
    /// Provides localized strings for the mod UI in all languages supported by Hollow Knight
    /// </summary>
    public static class ModLocalization
    {
        private static Dictionary<string, Dictionary<Language.LanguageCode, string>> translations = new Dictionary<string, Dictionary<Language.LanguageCode, string>>()
        {
            // Mod startup/shutdown messages
            ["MOD_LOADED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Hollow Knight Accessibility Mod loaded successfully",
                [Language.LanguageCode.ES] = "Mod de Accesibilidad de Hollow Knight cargado exitosamente",
                [Language.LanguageCode.PT] = "Mod de Acessibilidade do Hollow Knight carregado com sucesso",
                [Language.LanguageCode.IT] = "Mod di accessibilità di Hollow Knight caricato con successo",
                [Language.LanguageCode.FR] = "Mod d'accessibilité de Hollow Knight chargé avec succès",
                [Language.LanguageCode.DE] = "Hollow Knight Barrierefreiheit-Mod erfolgreich geladen",
                [Language.LanguageCode.RU] = "Мод специальных возможностей Hollow Knight успешно загружен",
                [Language.LanguageCode.ZH] = "空洞骑士无障碍模组加载成功",
                [Language.LanguageCode.JA] = "Hollow Knightアクセシビリティモッドが正常に読み込まれました",
                [Language.LanguageCode.KO] = "Hollow Knight 접근성 모드가 성공적으로 로드되었습니다"
            },
            ["MOD_UNLOADED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Hollow Knight Accessibility Mod unloaded",
                [Language.LanguageCode.ES] = "Mod de Accesibilidad de Hollow Knight descargado",
                [Language.LanguageCode.PT] = "Mod de Acessibilidade do Hollow Knight descarregado",
                [Language.LanguageCode.IT] = "Mod di accessibilità di Hollow Knight scaricato",
                [Language.LanguageCode.FR] = "Mod d'accessibilité de Hollow Knight déchargé",
                [Language.LanguageCode.DE] = "Hollow Knight Barrierefreiheit-Mod entladen",
                [Language.LanguageCode.RU] = "Мод специальных возможностей Hollow Knight выгружен",
                [Language.LanguageCode.ZH] = "空洞骑士无障碍模组已卸载",
                [Language.LanguageCode.JA] = "Hollow Knightアクセシビリティモッドがアンロードされました",
                [Language.LanguageCode.KO] = "Hollow Knight 접근성 모드가 언로드되었습니다"
            },

            // Inventory messages
            ["INVENTORY_OPENED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Inventory opened",
                [Language.LanguageCode.ES] = "Inventario abierto",
                [Language.LanguageCode.PT] = "Inventário aberto",
                [Language.LanguageCode.IT] = "Inventario aperto",
                [Language.LanguageCode.FR] = "Inventaire ouvert",
                [Language.LanguageCode.DE] = "Inventar geöffnet",
                [Language.LanguageCode.RU] = "Инвентарь открыт",
                [Language.LanguageCode.ZH] = "打开物品栏",
                [Language.LanguageCode.JA] = "インベントリを開きました",
                [Language.LanguageCode.KO] = "인벤토리 열림"
            },
            ["COST"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Cost",
                [Language.LanguageCode.ES] = "Costo",
                [Language.LanguageCode.PT] = "Custo",
                [Language.LanguageCode.IT] = "Costo",
                [Language.LanguageCode.FR] = "Coût",
                [Language.LanguageCode.DE] = "Kosten",
                [Language.LanguageCode.RU] = "Стоимость",
                [Language.LanguageCode.ZH] = "花费",
                [Language.LanguageCode.JA] = "コスト",
                [Language.LanguageCode.KO] = "비용"
            },
            ["NOTCH_SINGULAR"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "notch",
                [Language.LanguageCode.ES] = "muesca",
                [Language.LanguageCode.PT] = "entalhe",
                [Language.LanguageCode.IT] = "tacca",
                [Language.LanguageCode.FR] = "encoche",
                [Language.LanguageCode.DE] = "Kerbe",
                [Language.LanguageCode.RU] = "слот",
                [Language.LanguageCode.ZH] = "槽",
                [Language.LanguageCode.JA] = "スロット",
                [Language.LanguageCode.KO] = "홈"
            },
            ["NOTCH_PLURAL"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "notches",
                [Language.LanguageCode.ES] = "muescas",
                [Language.LanguageCode.PT] = "entalhes",
                [Language.LanguageCode.IT] = "tacche",
                [Language.LanguageCode.FR] = "encoches",
                [Language.LanguageCode.DE] = "Kerben",
                [Language.LanguageCode.RU] = "слота",
                [Language.LanguageCode.ZH] = "槽",
                [Language.LanguageCode.JA] = "スロット",
                [Language.LanguageCode.KO] = "홈"
            },
            ["CHARM"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Charm",
                [Language.LanguageCode.ES] = "Amuleto",
                [Language.LanguageCode.PT] = "Amuleto",
                [Language.LanguageCode.IT] = "Amuleto",
                [Language.LanguageCode.FR] = "Amulette",
                [Language.LanguageCode.DE] = "Amulett",
                [Language.LanguageCode.RU] = "Талисман",
                [Language.LanguageCode.ZH] = "护符",
                [Language.LanguageCode.JA] = "チャーム",
                [Language.LanguageCode.KO] = "부적"
            },

            // Toggle states
            ["ENABLED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "enabled",
                [Language.LanguageCode.ES] = "activado",
                [Language.LanguageCode.PT] = "ativado",
                [Language.LanguageCode.IT] = "attivato",
                [Language.LanguageCode.FR] = "activé",
                [Language.LanguageCode.DE] = "aktiviert",
                [Language.LanguageCode.RU] = "включено",
                [Language.LanguageCode.ZH] = "启用",
                [Language.LanguageCode.JA] = "有効",
                [Language.LanguageCode.KO] = "활성화"
            },
            ["DISABLED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "disabled",
                [Language.LanguageCode.ES] = "desactivado",
                [Language.LanguageCode.PT] = "desativado",
                [Language.LanguageCode.IT] = "disattivato",
                [Language.LanguageCode.FR] = "désactivé",
                [Language.LanguageCode.DE] = "deaktiviert",
                [Language.LanguageCode.RU] = "выключено",
                [Language.LanguageCode.ZH] = "禁用",
                [Language.LanguageCode.JA] = "無効",
                [Language.LanguageCode.KO] = "비활성화"
            },
            ["EQUIPPED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Equipped",
                [Language.LanguageCode.ES] = "Equipado",
                [Language.LanguageCode.PT] = "Equipado",
                [Language.LanguageCode.IT] = "Equipaggiato",
                [Language.LanguageCode.FR] = "Équipé",
                [Language.LanguageCode.DE] = "Ausgerüstet",
                [Language.LanguageCode.RU] = "Оснащен",
                [Language.LanguageCode.ZH] = "已装备",
                [Language.LanguageCode.JA] = "装備中",
                [Language.LanguageCode.KO] = "장착됨"
            },

            // Save slot states
            ["NEW_GAME"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "New game",
                [Language.LanguageCode.ES] = "Nuevo juego",
                [Language.LanguageCode.PT] = "Novo jogo",
                [Language.LanguageCode.IT] = "Nuova partita",
                [Language.LanguageCode.FR] = "Nouvelle partie",
                [Language.LanguageCode.DE] = "Neues Spiel",
                [Language.LanguageCode.RU] = "Новая игра",
                [Language.LanguageCode.ZH] = "新游戏",
                [Language.LanguageCode.JA] = "新しいゲーム",
                [Language.LanguageCode.KO] = "새 게임"
            },
            ["CORRUPTED_FILE"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Corrupted file",
                [Language.LanguageCode.ES] = "Archivo corrupto",
                [Language.LanguageCode.PT] = "Arquivo corrompido",
                [Language.LanguageCode.IT] = "File corrotto",
                [Language.LanguageCode.FR] = "Fichier corrompu",
                [Language.LanguageCode.DE] = "Beschädigte Datei",
                [Language.LanguageCode.RU] = "Поврежденный файл",
                [Language.LanguageCode.ZH] = "损坏的文件",
                [Language.LanguageCode.JA] = "破損したファイル",
                [Language.LanguageCode.KO] = "손상된 파일"
            },
            ["LOADING"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Loading",
                [Language.LanguageCode.ES] = "Cargando",
                [Language.LanguageCode.PT] = "Carregando",
                [Language.LanguageCode.IT] = "Caricamento",
                [Language.LanguageCode.FR] = "Chargement",
                [Language.LanguageCode.DE] = "Laden",
                [Language.LanguageCode.RU] = "Загрузка",
                [Language.LanguageCode.ZH] = "加载中",
                [Language.LanguageCode.JA] = "読み込み中",
                [Language.LanguageCode.KO] = "로딩 중"
            },

            // Game save messages
            ["SAVING_GAME"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Saving game",
                [Language.LanguageCode.ES] = "Guardando juego",
                [Language.LanguageCode.PT] = "Salvando jogo",
                [Language.LanguageCode.IT] = "Salvataggio",
                [Language.LanguageCode.FR] = "Sauvegarde",
                [Language.LanguageCode.DE] = "Speichern",
                [Language.LanguageCode.RU] = "Сохранение",
                [Language.LanguageCode.ZH] = "保存游戏",
                [Language.LanguageCode.JA] = "保存中",
                [Language.LanguageCode.KO] = "게임 저장 중"
            },
            ["GAME_SAVED"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Game saved",
                [Language.LanguageCode.ES] = "Juego guardado",
                [Language.LanguageCode.PT] = "Jogo salvo",
                [Language.LanguageCode.IT] = "Gioco salvato",
                [Language.LanguageCode.FR] = "Partie sauvegardée",
                [Language.LanguageCode.DE] = "Spiel gespeichert",
                [Language.LanguageCode.RU] = "Игра сохранена",
                [Language.LanguageCode.ZH] = "游戏已保存",
                [Language.LanguageCode.JA] = "保存されました",
                [Language.LanguageCode.KO] = "게임 저장됨"
            },

            // Player status messages
            ["HEALTH"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Health",
                [Language.LanguageCode.ES] = "Salud",
                [Language.LanguageCode.PT] = "Saúde",
                [Language.LanguageCode.IT] = "Salute",
                [Language.LanguageCode.FR] = "Santé",
                [Language.LanguageCode.DE] = "Gesundheit",
                [Language.LanguageCode.RU] = "Здоровье",
                [Language.LanguageCode.ZH] = "生命值",
                [Language.LanguageCode.JA] = "体力",
                [Language.LanguageCode.KO] = "체력"
            },
            ["SOUL"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Soul",
                [Language.LanguageCode.ES] = "Alma",
                [Language.LanguageCode.PT] = "Alma",
                [Language.LanguageCode.IT] = "Anima",
                [Language.LanguageCode.FR] = "Âme",
                [Language.LanguageCode.DE] = "Seele",
                [Language.LanguageCode.RU] = "Душа",
                [Language.LanguageCode.ZH] = "灵魂",
                [Language.LanguageCode.JA] = "ソウル",
                [Language.LanguageCode.KO] = "소울"
            },
            ["GEO"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Geo",
                [Language.LanguageCode.ES] = "Geo",
                [Language.LanguageCode.PT] = "Geo",
                [Language.LanguageCode.IT] = "Geo",
                [Language.LanguageCode.FR] = "Geo",
                [Language.LanguageCode.DE] = "Geo",
                [Language.LanguageCode.RU] = "Гео",
                [Language.LanguageCode.ZH] = "吉欧",
                [Language.LanguageCode.JA] = "ジオ",
                [Language.LanguageCode.KO] = "지오"
            },
            ["PLAYER_DATA_NOT_AVAILABLE"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Player data not available",
                [Language.LanguageCode.ES] = "Datos del jugador no disponibles",
                [Language.LanguageCode.PT] = "Dados do jogador não disponíveis",
                [Language.LanguageCode.IT] = "Dati del giocatore non disponibili",
                [Language.LanguageCode.FR] = "Données du joueur non disponibles",
                [Language.LanguageCode.DE] = "Spielerdaten nicht verfügbar",
                [Language.LanguageCode.RU] = "Данные игрока недоступны",
                [Language.LanguageCode.ZH] = "玩家数据不可用",
                [Language.LanguageCode.JA] = "プレイヤーデータは利用できません",
                [Language.LanguageCode.KO] = "플레이어 데이터를 사용할 수 없습니다"
            },
            ["GAME_MANAGER_NOT_FOUND"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Game manager not found",
                [Language.LanguageCode.ES] = "Gestor del juego no encontrado",
                [Language.LanguageCode.PT] = "Gerenciador do jogo não encontrado",
                [Language.LanguageCode.IT] = "Gestore di gioco non trovato",
                [Language.LanguageCode.FR] = "Gestionnaire de jeu introuvable",
                [Language.LanguageCode.DE] = "Spiel-Manager nicht gefunden",
                [Language.LanguageCode.RU] = "Менеджер игры не найден",
                [Language.LanguageCode.ZH] = "未找到游戏管理器",
                [Language.LanguageCode.JA] = "ゲームマネージャーが見つかりません",
                [Language.LanguageCode.KO] = "게임 매니저를 찾을 수 없습니다"
            },
            ["ERROR_GETTING_PLAYER_STATUS"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Error getting player status",
                [Language.LanguageCode.ES] = "Error al obtener el estado del jugador",
                [Language.LanguageCode.PT] = "Erro ao obter status do jogador",
                [Language.LanguageCode.IT] = "Errore nel recupero dello stato del giocatore",
                [Language.LanguageCode.FR] = "Erreur lors de l'obtention du statut du joueur",
                [Language.LanguageCode.DE] = "Fehler beim Abrufen des Spielerstatus",
                [Language.LanguageCode.RU] = "Ошибка получения статуса игрока",
                [Language.LanguageCode.ZH] = "获取玩家状态时出错",
                [Language.LanguageCode.JA] = "プレイヤーステータスの取得エラー",
                [Language.LanguageCode.KO] = "플레이어 상태를 가져오는 중 오류 발생"
            },

            // Slot label
            ["SLOT"] = new Dictionary<Language.LanguageCode, string>
            {
                [Language.LanguageCode.EN] = "Slot",
                [Language.LanguageCode.ES] = "Slot",
                [Language.LanguageCode.PT] = "Slot",
                [Language.LanguageCode.IT] = "Slot",
                [Language.LanguageCode.FR] = "Emplacement",
                [Language.LanguageCode.DE] = "Speicherplatz",
                [Language.LanguageCode.RU] = "Слот",
                [Language.LanguageCode.ZH] = "存档位",
                [Language.LanguageCode.JA] = "スロット",
                [Language.LanguageCode.KO] = "슬롯"
            }
        };

        /// <summary>
        /// Get a localized string for the current game language
        /// </summary>
        /// <param name="key">The localization key</param>
        /// <returns>Localized string, or the key itself if not found</returns>
        public static string Get(string key)
        {
            Language.LanguageCode currentLang = Language.Language.CurrentLanguage();

            if (translations.ContainsKey(key) && translations[key].ContainsKey(currentLang))
            {
                return translations[key][currentLang];
            }

            // Fallback to English if current language not found
            if (translations.ContainsKey(key) && translations[key].ContainsKey(Language.LanguageCode.EN))
            {
                Plugin.Logger.LogWarning($"[ModLocalization] No translation for key '{key}' in language '{currentLang}', using English fallback");
                return translations[key][Language.LanguageCode.EN];
            }

            // If all else fails, return the key
            Plugin.Logger.LogError($"[ModLocalization] Missing translation key: '{key}'");
            return key;
        }

        /// <summary>
        /// Get a formatted localized string
        /// </summary>
        public static string Format(string key, params object[] args)
        {
            string template = Get(key);
            return string.Format(template, args);
        }
    }
}
