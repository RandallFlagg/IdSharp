using System.Collections.Generic;

namespace IdSharp.Tagging.ID3v2;

/// <summary>
/// Static helper class for ISO-639-2 languages.
/// </summary>
public static class LanguageHelper
{
    #region <<< Sorted languages >>>

    #endregion <<< Sorted languages >>>

    /// <summary>
    /// Gets a dictionary containing 3-letter ISO-639-2 language codes as the key and the English version of the language name as the value.
    /// </summary>
    /// <value>A dictionary containing 3-letter ISO-639-2 language codes as the key and the English version of the language name as the value.</value>
    public static Dictionary<string, string> Languages { get; private set; }

    /// <summary>
    /// Gets a string array of the English version of the language names sorted alphabetically.
    /// </summary>
    /// <value>A string array of the English version of the language names sorted alphabetically.</value>
    public static string[] SortedLanguages { get; } = new[]{
                                                                         "Abkhazian",
                                                                         "Achinese",
                                                                         "Acoli",
                                                                         "Adangme",
                                                                         "Afar",
                                                                         "Afrihili",
                                                                         "Afrikaans",
                                                                         "Afro-Asiatic (Other)",
                                                                         "Akan",
                                                                         "Akkadian",
                                                                         "Albanian",
                                                                         "Aleut",
                                                                         "Algonquian Languages",
                                                                         "Altaic (Other)",
                                                                         "Amharic",
                                                                         "Apache Languages",
                                                                         "Arabic",
                                                                         "Aramaic",
                                                                         "Arapaho",
                                                                         "Araucanian",
                                                                         "Arawak",
                                                                         "Armenian",
                                                                         "Artificial (Other)",
                                                                         "Assamese",
                                                                         "Athapascan Languages",
                                                                         "Austronesian (Other)",
                                                                         "Avaric",
                                                                         "Avestan",
                                                                         "Awadhi",
                                                                         "Aymara",
                                                                         "Azerbaijani",
                                                                         "Aztec",
                                                                         "Balinese",
                                                                         "Baltic (Other)",
                                                                         "Baluchi",
                                                                         "Bambara",
                                                                         "Bamileke Languages",
                                                                         "Banda",
                                                                         "Bantu (Other)",
                                                                         "Basa",
                                                                         "Bashkir",
                                                                         "Basque",
                                                                         "Beja",
                                                                         "Bemba",
                                                                         "Bengali",
                                                                         "Berber (Other)",
                                                                         "Bhojpuri",
                                                                         "Bihari",
                                                                         "Bikol",
                                                                         "Bini",
                                                                         "Bislama",
                                                                         "Bosnian",
                                                                         "Braj",
                                                                         "Breton",
                                                                         "Buginese",
                                                                         "Bulgarian",
                                                                         "Buriat",
                                                                         "Burmese",
                                                                         "Byelorussian",
                                                                         "Caddo",
                                                                         "Carib",
                                                                         "Catalan",
                                                                         "Caucasian (Other)",
                                                                         "Cebuano",
                                                                         "Celtic (Other)",
                                                                         "Central American Indian (Other)",
                                                                         "Chagatai",
                                                                         "Chamorro",
                                                                         "Chechen",
                                                                         "Cherokee",
                                                                         "Cheyenne",
                                                                         "Chibcha",
                                                                         "Chinese",
                                                                         "Chinook jargon",
                                                                         "Choctaw",
                                                                         "Church Slavic",
                                                                         "Chuvash",
                                                                         "Coptic",
                                                                         "Cornish",
                                                                         "Corsican",
                                                                         "Cree",
                                                                         "Creek",
                                                                         "Creoles and Pidgins (Other)",
                                                                         "Creoles and Pidgins, English-based (Other)",
                                                                         "Creoles and Pidgins, French-based (Other)",
                                                                         "Creoles and Pidgins, Portuguese-based (Other)",
                                                                         "Croatian",
                                                                         "Cushitic (Other)",
                                                                         "Czech",
                                                                         "Dakota",
                                                                         "Danish",
                                                                         "Delaware",
                                                                         "Dinka",
                                                                         "Divehi",
                                                                         "Dogri",
                                                                         "Dravidian (Other)",
                                                                         "Duala",
                                                                         "Dutch",
                                                                         "Dutch, Middle (ca. 1050-1350)",
                                                                         "Dyula",
                                                                         "Dzongkha",
                                                                         "Efik",
                                                                         "Egyptian (Ancient)",
                                                                         "Ekajuk",
                                                                         "Elamite",
                                                                         "English",
                                                                         "English, Old (ca. 450-1100)",
                                                                         "Eskimo (Other)",
                                                                         "Esperanto",
                                                                         "Estonian",
                                                                         "Ewe",
                                                                         "Ewondo",
                                                                         "Fang",
                                                                         "Fanti",
                                                                         "Faroese",
                                                                         "Fijian",
                                                                         "Finnish",
                                                                         "Finno-Ugrian (Other)",
                                                                         "Fon",
                                                                         "French",
                                                                         "French, Middle (ca. 1400-1600)",
                                                                         "French, Old (842- ca. 1400)",
                                                                         "Frisian",
                                                                         "Fulah",
                                                                         "Ga",
                                                                         "Gaelic (Scots)",
                                                                         "Gallegan",
                                                                         "Ganda",
                                                                         "Gayo",
                                                                         "Geez",
                                                                         "Georgian",
                                                                         "German",
                                                                         "German, Middle High (ca. 1050-1500)",
                                                                         "German, Old High (ca. 750-1050)",
                                                                         "Germanic (Other)",
                                                                         "Gilbertese",
                                                                         "Gondi",
                                                                         "Gothic",
                                                                         "Grebo",
                                                                         "Greek",
                                                                         "Greek, Ancient (to 1453)",
                                                                         "Greenlandic",
                                                                         "Guarani",
                                                                         "Gujarati",
                                                                         "Haida",
                                                                         "Hausa",
                                                                         "Hawaiian",
                                                                         "Hebrew",
                                                                         "Herero",
                                                                         "Hiligaynon",
                                                                         "Himachali",
                                                                         "Hindi",
                                                                         "Hiri Motu",
                                                                         "Hungarian",
                                                                         "Hupa",
                                                                         "Iban",
                                                                         "Icelandic",
                                                                         "Igbo",
                                                                         "Ijo",
                                                                         "Iloko",
                                                                         "Indic (Other)",
                                                                         "Indo-European (Other) Interlingue",
                                                                         "Indonesian",
                                                                         "Interlingua (International Auxiliary language Association)",
                                                                         "Inuktitut",
                                                                         "Inupiak",
                                                                         "Iranian (Other)",
                                                                         "Irish",
                                                                         "Irish, Middle (900 - 1200)",
                                                                         "Irish, Old (to 900)",
                                                                         "Iroquoian uages",
                                                                         "Italian",
                                                                         "Japanese",
                                                                         "Javanese",
                                                                         "Judeo-Arabic",
                                                                         "Judeo-Persian",
                                                                         "Kabyle",
                                                                         "Kachin",
                                                                         "Kamba",
                                                                         "Kannada",
                                                                         "Kanuri",
                                                                         "Kara-Kalpak",
                                                                         "Karen",
                                                                         "Kashmiri",
                                                                         "Kawi",
                                                                         "Kazakh",
                                                                         "Khasi",
                                                                         "Khmer",
                                                                         "Khoisan (Other)",
                                                                         "Khotanese",
                                                                         "Kikuyu",
                                                                         "Kinyarwanda",
                                                                         "Kirghiz",
                                                                         "Komi",
                                                                         "Kongo",
                                                                         "Konkani",
                                                                         "Korean",
                                                                         "Kpelle",
                                                                         "Kru",
                                                                         "Kuanyama",
                                                                         "Kumyk",
                                                                         "Kurdish",
                                                                         "Kurukh",
                                                                         "Kusaie",
                                                                         "Kutenai",
                                                                         "Ladino",
                                                                         "Lahnda",
                                                                         "Lamba",
                                                                         "Langue d'Oc (post 1500)",
                                                                         "Lao",
                                                                         "Latin",
                                                                         "Latvian",
                                                                         "Letzeburgesch",
                                                                         "Lezghian",
                                                                         "Lingala",
                                                                         "Lithuanian",
                                                                         "Lozi",
                                                                         "Luba-Katanga",
                                                                         "Luiseno",
                                                                         "Lunda",
                                                                         "Luo (Kenya and Tanzania)",
                                                                         "Macedonian",
                                                                         "Macedonian Makasar",
                                                                         "Madurese",
                                                                         "Magahi",
                                                                         "Maithili",
                                                                         "Malagasy",
                                                                         "Malay",
                                                                         "Malayalam",
                                                                         "Maltese",
                                                                         "Mandingo",
                                                                         "Manipuri",
                                                                         "Manobo Languages",
                                                                         "Manx",
                                                                         "Maori",
                                                                         "Marathi",
                                                                         "Mari",
                                                                         "Marshall",
                                                                         "Marwari",
                                                                         "Masai",
                                                                         "Mayan Languages",
                                                                         "Mende",
                                                                         "Micmac",
                                                                         "Middle English (ca. 1100-1500)",
                                                                         "Minangkabau",
                                                                         "Miscellaneous (Other)",
                                                                         "Mohawk",
                                                                         "Moldavian",
                                                                         "Mongo",
                                                                         "Mongolian",
                                                                         "Mon-Kmer (Other)",
                                                                         "Mossi",
                                                                         "Multiple Languages",
                                                                         "Munda Languages",
                                                                         "Nauru",
                                                                         "Navajo",
                                                                         "Ndebele, North",
                                                                         "Ndebele, South",
                                                                         "Ndongo",
                                                                         "Nepali",
                                                                         "Newari",
                                                                         "Niger-Kordofanian (Other)",
                                                                         "Nilo-Saharan (Other)",
                                                                         "Niuean",
                                                                         "Norse, Old",
                                                                         "North American Indian (Other)",
                                                                         "Norwegian",
                                                                         "Norwegian (Nynorsk)",
                                                                         "Nubian Languages",
                                                                         "Nyamwezi",
                                                                         "Nyanja",
                                                                         "Nyankole",
                                                                         "Nyoro",
                                                                         "Nzima",
                                                                         "Ojibwa",
                                                                         "Oriya",
                                                                         "Oromo",
                                                                         "Osage",
                                                                         "Ossetic",
                                                                         "Otomian Languages",
                                                                         "Pahlavi",
                                                                         "Palauan",
                                                                         "Pali",
                                                                         "Pampanga",
                                                                         "Pangasinan",
                                                                         "Panjabi",
                                                                         "Papiamento",
                                                                         "Papuan-Australian (Other)",
                                                                         "Persian",
                                                                         "Persian, Old (ca 600 - 400 B.C.)",
                                                                         "Phoenician",
                                                                         "Polish",
                                                                         "Ponape",
                                                                         "Portuguese",
                                                                         "Prakrit uages",
                                                                         "Provencal, Old (to 1500)",
                                                                         "Pushto",
                                                                         "Quechua",
                                                                         "Rajasthani",
                                                                         "Rarotongan",
                                                                         "Rhaeto-Romance",
                                                                         "Romance (Other)",
                                                                         "Romanian",
                                                                         "Romany",
                                                                         "Rundi",
                                                                         "Russian",
                                                                         "Salishan Languages",
                                                                         "Samaritan Aramaic",
                                                                         "Sami Languages",
                                                                         "Samoan",
                                                                         "Sandawe",
                                                                         "Sango",
                                                                         "Sanskrit",
                                                                         "Sardinian",
                                                                         "Scots",
                                                                         "Selkup",
                                                                         "Semitic (Other)",
                                                                         "Serbian",
                                                                         "Serer",
                                                                         "Shan",
                                                                         "Shona",
                                                                         "Sidamo",
                                                                         "Siksika",
                                                                         "Sindhi",
                                                                         "Singhalese",
                                                                         "Sino-Tibetan (Other)",
                                                                         "Siouan Languages",
                                                                         "Siswant Swazi",
                                                                         "Slavic (Other)",
                                                                         "Slovak",
                                                                         "Slovenian",
                                                                         "Sogdian",
                                                                         "Somali",
                                                                         "Songhai",
                                                                         "Sorbian Languages",
                                                                         "Sotho, Northern",
                                                                         "Sotho, Southern",
                                                                         "South American Indian (Other)",
                                                                         "Spanish",
                                                                         "Sudanese",
                                                                         "Sukuma",
                                                                         "Sumerian",
                                                                         "Susu",
                                                                         "Swahili",
                                                                         "Swedish",
                                                                         "Syriac",
                                                                         "Tagalog",
                                                                         "Tahitian",
                                                                         "Tajik",
                                                                         "Tamashek",
                                                                         "Tamil",
                                                                         "Tatar",
                                                                         "Telugu",
                                                                         "Tereno",
                                                                         "Thai",
                                                                         "Tibetan",
                                                                         "Tigre",
                                                                         "Tigrinya",
                                                                         "Timne",
                                                                         "Tivi",
                                                                         "Tlingit",
                                                                         "Tokelau",
                                                                         "Tonga (Nyasa)",
                                                                         "Tonga (Tonga Islands)",
                                                                         "Truk",
                                                                         "Tsimshian",
                                                                         "Tsonga",
                                                                         "Tswana",
                                                                         "Tumbuka",
                                                                         "Turkish",
                                                                         "Turkish, Ottoman (1500 - 1928)",
                                                                         "Turkmen",
                                                                         "Tuvinian",
                                                                         "Twi",
                                                                         "Ugaritic",
                                                                         "Uighur",
                                                                         "Ukrainian",
                                                                         "Umbundu",
                                                                         "Undetermined",
                                                                         "Urdu",
                                                                         "Uzbek",
                                                                         "Vai",
                                                                         "Venda",
                                                                         "Vietnamese",
                                                                         "Volapük",
                                                                         "Votic",
                                                                         "Wakashan Languages",
                                                                         "Walamo",
                                                                         "Waray",
                                                                         "Washo",
                                                                         "Welsh",
                                                                         "Wolof",
                                                                         "Xhosa",
                                                                         "Yakut",
                                                                         "Yao",
                                                                         "Yap",
                                                                         "Yiddish",
                                                                         "Yoruba",
                                                                         "Zapotec",
                                                                         "Zenaga",
                                                                         "Zhuang",
                                                                         "Zulu",
                                                                         "Zuni"
                                                                     };

    static LanguageHelper()
    {
        #region <<< Language code / English description dictionary

        Languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Languages.Add("aar", "Afar");
        Languages.Add("abk", "Abkhazian");
        Languages.Add("ace", "Achinese");
        Languages.Add("ach", "Acoli");
        Languages.Add("ada", "Adangme");
        Languages.Add("afa", "Afro-Asiatic (Other)");
        Languages.Add("afh", "Afrihili");
        Languages.Add("afr", "Afrikaans");
        Languages.Add("aka", "Akan");
        Languages.Add("akk", "Akkadian");
        Languages.Add("alb", "Albanian");
        Languages.Add("ale", "Aleut");
        Languages.Add("alg", "Algonquian Languages");
        Languages.Add("amh", "Amharic");
        Languages.Add("ang", "English, Old (ca. 450-1100)");
        Languages.Add("apa", "Apache Languages");
        Languages.Add("ara", "Arabic");
        Languages.Add("arc", "Aramaic");
        Languages.Add("arm", "Armenian");
        Languages.Add("arn", "Araucanian");
        Languages.Add("arp", "Arapaho");
        Languages.Add("art", "Artificial (Other)");
        Languages.Add("arw", "Arawak");
        Languages.Add("asm", "Assamese");
        Languages.Add("ath", "Athapascan Languages");
        Languages.Add("ava", "Avaric");
        Languages.Add("ave", "Avestan");
        Languages.Add("awa", "Awadhi");
        Languages.Add("aym", "Aymara");
        Languages.Add("aze", "Azerbaijani");
        Languages.Add("bad", "Banda");
        Languages.Add("bai", "Bamileke Languages");
        Languages.Add("bak", "Bashkir");
        Languages.Add("bal", "Baluchi");
        Languages.Add("bam", "Bambara");
        Languages.Add("ban", "Balinese");
        Languages.Add("baq", "Basque");
        Languages.Add("bas", "Basa");
        Languages.Add("bat", "Baltic (Other)");
        Languages.Add("bej", "Beja");
        Languages.Add("bel", "Byelorussian");
        Languages.Add("bem", "Bemba");
        Languages.Add("ben", "Bengali");
        Languages.Add("ber", "Berber (Other)");
        Languages.Add("bho", "Bhojpuri");
        Languages.Add("bih", "Bihari");
        Languages.Add("bik", "Bikol");
        Languages.Add("bin", "Bini");
        Languages.Add("bis", "Bislama");
        Languages.Add("bla", "Siksika");
        Languages.Add("bnt", "Bantu (Other)");
        Languages.Add("bod", "Tibetan");
        Languages.Add("bra", "Braj");
        Languages.Add("bre", "Breton");
        Languages.Add("bua", "Buriat");
        Languages.Add("bug", "Buginese");
        Languages.Add("bul", "Bulgarian");
        Languages.Add("bur", "Burmese");
        Languages.Add("cad", "Caddo");
        Languages.Add("cai", "Central American Indian (Other)");
        Languages.Add("car", "Carib");
        Languages.Add("cat", "Catalan");
        Languages.Add("cau", "Caucasian (Other)");
        Languages.Add("ceb", "Cebuano");
        Languages.Add("cel", "Celtic (Other)");
        Languages.Add("ces", "Czech");
        Languages.Add("cha", "Chamorro");
        Languages.Add("chb", "Chibcha");
        Languages.Add("che", "Chechen");
        Languages.Add("chg", "Chagatai");
        Languages.Add("chi", "Chinese");
        Languages.Add("chm", "Mari");
        Languages.Add("chn", "Chinook jargon");
        Languages.Add("cho", "Choctaw");
        Languages.Add("chr", "Cherokee");
        Languages.Add("chu", "Church Slavic");
        Languages.Add("chv", "Chuvash");
        Languages.Add("chy", "Cheyenne");
        Languages.Add("cop", "Coptic");
        Languages.Add("cor", "Cornish");
        Languages.Add("cos", "Corsican");
        Languages.Add("cpe", "Creoles and Pidgins, English-based (Other)");
        Languages.Add("cpf", "Creoles and Pidgins, French-based (Other)");
        Languages.Add("cpp", "Creoles and Pidgins, Portuguese-based (Other)");
        Languages.Add("cre", "Cree");
        Languages.Add("crp", "Creoles and Pidgins (Other)");
        Languages.Add("cus", "Cushitic (Other)");
        Languages.Add("cym", "Welsh");
        Languages.Add("cze", "Czech");
        Languages.Add("dak", "Dakota");
        Languages.Add("dan", "Danish");
        Languages.Add("del", "Delaware");
        Languages.Add("deu", "German");
        Languages.Add("din", "Dinka");
        Languages.Add("div", "Divehi");
        Languages.Add("doi", "Dogri");
        Languages.Add("dra", "Dravidian (Other)");
        Languages.Add("dua", "Duala");
        Languages.Add("dum", "Dutch, Middle (ca. 1050-1350)");
        Languages.Add("dut", "Dutch");
        Languages.Add("dyu", "Dyula");
        Languages.Add("dzo", "Dzongkha");
        Languages.Add("efi", "Efik");
        Languages.Add("egy", "Egyptian (Ancient)");
        Languages.Add("eka", "Ekajuk");
        Languages.Add("ell", "Greek");
        Languages.Add("elx", "Elamite");
        Languages.Add("eng", "English");
        Languages.Add("enm", "Middle English (ca. 1100-1500)");
        Languages.Add("epo", "Esperanto");
        Languages.Add("esk", "Eskimo (Other)");
        Languages.Add("esl", "Spanish");
        Languages.Add("est", "Estonian");
        Languages.Add("eus", "Basque");
        Languages.Add("ewe", "Ewe");
        Languages.Add("ewo", "Ewondo");
        Languages.Add("fan", "Fang");
        Languages.Add("fao", "Faroese");
        Languages.Add("fas", "Persian");
        Languages.Add("fat", "Fanti");
        Languages.Add("fij", "Fijian");
        Languages.Add("fin", "Finnish");
        Languages.Add("fiu", "Finno-Ugrian (Other)");
        Languages.Add("fon", "Fon");
        Languages.Add("fra", "French");
        Languages.Add("fre", "French");
        Languages.Add("frm", "French, Middle (ca. 1400-1600)");
        Languages.Add("fro", "French, Old (842- ca. 1400)");
        Languages.Add("fry", "Frisian");
        Languages.Add("ful", "Fulah");
        Languages.Add("gaa", "Ga");
        Languages.Add("gae", "Gaelic (Scots)");
        Languages.Add("gai", "Irish");
        Languages.Add("gay", "Gayo");
        Languages.Add("gdh", "Gaelic (Scots)");
        Languages.Add("gem", "Germanic (Other)");
        Languages.Add("geo", "Georgian");
        Languages.Add("ger", "German");
        Languages.Add("gez", "Geez");
        Languages.Add("gil", "Gilbertese");
        Languages.Add("glg", "Gallegan");
        Languages.Add("gmh", "German, Middle High (ca. 1050-1500)");
        Languages.Add("goh", "German, Old High (ca. 750-1050)");
        Languages.Add("gon", "Gondi");
        Languages.Add("got", "Gothic");
        Languages.Add("grb", "Grebo");
        Languages.Add("grc", "Greek, Ancient (to 1453)");
        Languages.Add("gre", "Greek");
        Languages.Add("grn", "Guarani");
        Languages.Add("guj", "Gujarati");
        Languages.Add("hai", "Haida");
        Languages.Add("hau", "Hausa");
        Languages.Add("haw", "Hawaiian");
        Languages.Add("heb", "Hebrew");
        Languages.Add("her", "Herero");
        Languages.Add("hil", "Hiligaynon");
        Languages.Add("him", "Himachali");
        Languages.Add("hin", "Hindi");
        Languages.Add("hmo", "Hiri Motu");
        Languages.Add("hun", "Hungarian");
        Languages.Add("hup", "Hupa");
        Languages.Add("hye", "Armenian");
        Languages.Add("iba", "Iban");
        Languages.Add("ibo", "Igbo");
        Languages.Add("ice", "Icelandic");
        Languages.Add("ijo", "Ijo");
        Languages.Add("iku", "Inuktitut");
        Languages.Add("ilo", "Iloko");
        Languages.Add("ina", "Interlingua (International Auxiliary language Association)");
        Languages.Add("inc", "Indic (Other)");
        Languages.Add("ind", "Indonesian");
        Languages.Add("ine", "Indo-European (Other) Interlingue");
        Languages.Add("ipk", "Inupiak");
        Languages.Add("ira", "Iranian (Other)");
        Languages.Add("iri", "Irish");
        Languages.Add("iro", "Iroquoian uages");
        Languages.Add("isl", "Icelandic");
        Languages.Add("ita", "Italian");
        Languages.Add("jav", "Javanese");
        Languages.Add("jaw", "Javanese");
        Languages.Add("jpn", "Japanese");
        Languages.Add("jpr", "Judeo-Persian");
        Languages.Add("jrb", "Judeo-Arabic");
        Languages.Add("kaa", "Kara-Kalpak");
        Languages.Add("kab", "Kabyle");
        Languages.Add("kac", "Kachin");
        Languages.Add("kal", "Greenlandic");
        Languages.Add("kam", "Kamba");
        Languages.Add("kan", "Kannada");
        Languages.Add("kar", "Karen");
        Languages.Add("kas", "Kashmiri");
        Languages.Add("kat", "Georgian");
        Languages.Add("kau", "Kanuri");
        Languages.Add("kaw", "Kawi");
        Languages.Add("kaz", "Kazakh");
        Languages.Add("kha", "Khasi");
        Languages.Add("khi", "Khoisan (Other)");
        Languages.Add("khm", "Khmer");
        Languages.Add("kho", "Khotanese");
        Languages.Add("kik", "Kikuyu");
        Languages.Add("kin", "Kinyarwanda");
        Languages.Add("kir", "Kirghiz");
        Languages.Add("kok", "Konkani");
        Languages.Add("kom", "Komi");
        Languages.Add("kon", "Kongo");
        Languages.Add("kor", "Korean");
        Languages.Add("kpe", "Kpelle");
        Languages.Add("kro", "Kru");
        Languages.Add("kru", "Kurukh");
        Languages.Add("kua", "Kuanyama");
        Languages.Add("kum", "Kumyk");
        Languages.Add("kur", "Kurdish");
        Languages.Add("kus", "Kusaie");
        Languages.Add("kut", "Kutenai");
        Languages.Add("lad", "Ladino");
        Languages.Add("lah", "Lahnda");
        Languages.Add("lam", "Lamba");
        Languages.Add("lao", "Lao");
        Languages.Add("lat", "Latin");
        Languages.Add("lav", "Latvian");
        Languages.Add("lez", "Lezghian");
        Languages.Add("lin", "Lingala");
        Languages.Add("lit", "Lithuanian");
        Languages.Add("lol", "Mongo");
        Languages.Add("loz", "Lozi");
        Languages.Add("ltz", "Letzeburgesch");
        Languages.Add("lub", "Luba-Katanga");
        Languages.Add("lug", "Ganda");
        Languages.Add("lui", "Luiseno");
        Languages.Add("lun", "Lunda");
        Languages.Add("luo", "Luo (Kenya and Tanzania)");
        Languages.Add("mac", "Macedonian");
        Languages.Add("mad", "Madurese");
        Languages.Add("mag", "Magahi");
        Languages.Add("mah", "Marshall");
        Languages.Add("mai", "Maithili");
        Languages.Add("mak", "Macedonian Makasar");
        Languages.Add("mal", "Malayalam");
        Languages.Add("man", "Mandingo");
        Languages.Add("mao", "Maori");
        Languages.Add("map", "Austronesian (Other)");
        Languages.Add("mar", "Marathi");
        Languages.Add("mas", "Masai");
        Languages.Add("max", "Manx");
        Languages.Add("may", "Malay");
        Languages.Add("men", "Mende");
        Languages.Add("mga", "Irish, Middle (900 - 1200)");
        Languages.Add("mic", "Micmac");
        Languages.Add("min", "Minangkabau");
        Languages.Add("mis", "Miscellaneous (Other)");
        Languages.Add("mkh", "Mon-Kmer (Other)");
        Languages.Add("mlg", "Malagasy");
        Languages.Add("mlt", "Maltese");
        Languages.Add("mni", "Manipuri");
        Languages.Add("mno", "Manobo Languages");
        Languages.Add("moh", "Mohawk");
        Languages.Add("mol", "Moldavian");
        Languages.Add("mon", "Mongolian");
        Languages.Add("mos", "Mossi");
        Languages.Add("mri", "Maori");
        Languages.Add("msa", "Malay");
        Languages.Add("mul", "Multiple Languages");
        Languages.Add("mun", "Munda Languages");
        Languages.Add("mus", "Creek");
        Languages.Add("mwr", "Marwari");
        Languages.Add("mya", "Burmese");
        Languages.Add("myn", "Mayan Languages");
        Languages.Add("nah", "Aztec");
        Languages.Add("nai", "North American Indian (Other)");
        Languages.Add("nau", "Nauru");
        Languages.Add("nav", "Navajo");
        Languages.Add("nbl", "Ndebele, South");
        Languages.Add("nde", "Ndebele, North");
        Languages.Add("ndo", "Ndongo");
        Languages.Add("nep", "Nepali");
        Languages.Add("new", "Newari");
        Languages.Add("nic", "Niger-Kordofanian (Other)");
        Languages.Add("niu", "Niuean");
        Languages.Add("nla", "Dutch");
        Languages.Add("nno", "Norwegian (Nynorsk)");
        Languages.Add("non", "Norse, Old");
        Languages.Add("nor", "Norwegian");
        Languages.Add("nso", "Sotho, Northern");
        Languages.Add("nub", "Nubian Languages");
        Languages.Add("nya", "Nyanja");
        Languages.Add("nym", "Nyamwezi");
        Languages.Add("nyn", "Nyankole");
        Languages.Add("nyo", "Nyoro");
        Languages.Add("nzi", "Nzima");
        Languages.Add("oci", "Langue d'Oc (post 1500)");
        Languages.Add("oji", "Ojibwa");
        Languages.Add("ori", "Oriya");
        Languages.Add("orm", "Oromo");
        Languages.Add("osa", "Osage");
        Languages.Add("oss", "Ossetic");
        Languages.Add("ota", "Turkish, Ottoman (1500 - 1928)");
        Languages.Add("oto", "Otomian Languages");
        Languages.Add("paa", "Papuan-Australian (Other)");
        Languages.Add("pag", "Pangasinan");
        Languages.Add("pal", "Pahlavi");
        Languages.Add("pam", "Pampanga");
        Languages.Add("pan", "Panjabi");
        Languages.Add("pap", "Papiamento");
        Languages.Add("pau", "Palauan");
        Languages.Add("peo", "Persian, Old (ca 600 - 400 B.C.)");
        Languages.Add("per", "Persian");
        Languages.Add("phn", "Phoenician");
        Languages.Add("pli", "Pali");
        Languages.Add("pol", "Polish");
        Languages.Add("pon", "Ponape");
        Languages.Add("por", "Portuguese");
        Languages.Add("pra", "Prakrit uages");
        Languages.Add("pro", "Provencal, Old (to 1500)");
        Languages.Add("pus", "Pushto");
        Languages.Add("que", "Quechua");
        Languages.Add("raj", "Rajasthani");
        Languages.Add("rar", "Rarotongan");
        Languages.Add("roa", "Romance (Other)");
        Languages.Add("roh", "Rhaeto-Romance");
        Languages.Add("rom", "Romany");
        Languages.Add("ron", "Romanian");
        Languages.Add("rum", "Romanian");
        Languages.Add("run", "Rundi");
        Languages.Add("rus", "Russian");
        Languages.Add("sad", "Sandawe");
        Languages.Add("sag", "Sango");
        Languages.Add("sah", "Yakut");
        Languages.Add("sai", "South American Indian (Other)");
        Languages.Add("sal", "Salishan Languages");
        Languages.Add("sam", "Samaritan Aramaic");
        Languages.Add("san", "Sanskrit");
        Languages.Add("sco", "Scots");
        Languages.Add("scr", "Croatian");
        Languages.Add("sel", "Selkup");
        Languages.Add("sem", "Semitic (Other)");
        Languages.Add("sga", "Irish, Old (to 900)");
        Languages.Add("shn", "Shan");
        Languages.Add("sid", "Sidamo");
        Languages.Add("sin", "Singhalese");
        Languages.Add("sio", "Siouan Languages");
        Languages.Add("sit", "Sino-Tibetan (Other)");
        Languages.Add("sla", "Slavic (Other)");
        Languages.Add("slk", "Slovak");
        Languages.Add("slo", "Slovak");
        Languages.Add("slv", "Slovenian");
        Languages.Add("smi", "Sami Languages");
        Languages.Add("smo", "Samoan");
        Languages.Add("sna", "Shona");
        Languages.Add("snd", "Sindhi");
        Languages.Add("sog", "Sogdian");
        Languages.Add("som", "Somali");
        Languages.Add("son", "Songhai");
        Languages.Add("sot", "Sotho, Southern");
        Languages.Add("spa", "Spanish");
        Languages.Add("sqi", "Albanian");
        Languages.Add("srd", "Sardinian");
        Languages.Add("srr", "Serer");
        Languages.Add("ssa", "Nilo-Saharan (Other)");
        Languages.Add("ssw", "Siswant Swazi");
        Languages.Add("suk", "Sukuma");
        Languages.Add("sun", "Sudanese");
        Languages.Add("sus", "Susu");
        Languages.Add("sux", "Sumerian");
        Languages.Add("sve", "Swedish");
        Languages.Add("swa", "Swahili");
        Languages.Add("swe", "Swedish");
        Languages.Add("syr", "Syriac");
        Languages.Add("tah", "Tahitian");
        Languages.Add("tam", "Tamil");
        Languages.Add("tat", "Tatar");
        Languages.Add("tel", "Telugu");
        Languages.Add("tem", "Timne");
        Languages.Add("ter", "Tereno");
        Languages.Add("tgk", "Tajik");
        Languages.Add("tgl", "Tagalog");
        Languages.Add("tha", "Thai");
        Languages.Add("tib", "Tibetan");
        Languages.Add("tig", "Tigre");
        Languages.Add("tir", "Tigrinya");
        Languages.Add("tiv", "Tivi");
        Languages.Add("tli", "Tlingit");
        Languages.Add("tmh", "Tamashek");
        Languages.Add("tog", "Tonga (Nyasa)");
        Languages.Add("ton", "Tonga (Tonga Islands)");
        Languages.Add("tru", "Truk");
        Languages.Add("tsi", "Tsimshian");
        Languages.Add("tsn", "Tswana");
        Languages.Add("tso", "Tsonga");
        Languages.Add("tuk", "Turkmen");
        Languages.Add("tum", "Tumbuka");
        Languages.Add("tur", "Turkish");
        Languages.Add("tut", "Altaic (Other)");
        Languages.Add("twi", "Twi");
        Languages.Add("tyv", "Tuvinian");
        Languages.Add("uga", "Ugaritic");
        Languages.Add("uig", "Uighur");
        Languages.Add("ukr", "Ukrainian");
        Languages.Add("umb", "Umbundu");
        Languages.Add("und", "Undetermined");
        Languages.Add("urd", "Urdu");
        Languages.Add("uzb", "Uzbek");
        Languages.Add("vai", "Vai");
        Languages.Add("ven", "Venda");
        Languages.Add("vie", "Vietnamese");
        Languages.Add("vol", "Volapük");
        Languages.Add("vot", "Votic");
        Languages.Add("wak", "Wakashan Languages");
        Languages.Add("wal", "Walamo");
        Languages.Add("war", "Waray");
        Languages.Add("was", "Washo");
        Languages.Add("wel", "Welsh");
        Languages.Add("wen", "Sorbian Languages");
        Languages.Add("wol", "Wolof");
        Languages.Add("xho", "Xhosa");
        Languages.Add("yao", "Yao");
        Languages.Add("yap", "Yap");
        Languages.Add("yid", "Yiddish");
        Languages.Add("yor", "Yoruba");
        Languages.Add("zap", "Zapotec");
        Languages.Add("zen", "Zenaga");
        Languages.Add("zha", "Zhuang");
        Languages.Add("zho", "Chinese");
        Languages.Add("zul", "Zulu");
        Languages.Add("zun", "Zuni");
        Languages.Add("tkl", "Tokelau");
        Languages.Add("hrv", "Croatian");
        Languages.Add("bos", "Bosnian");
        Languages.Add("scc", "Serbian");
        Languages.Add("srp", "Serbian");

        #endregion <<< Language code / English description dictionary
    }
}
